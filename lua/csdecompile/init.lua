require'plenary.job'
local pickers = require "telescope.pickers"
local finders = require "telescope.finders"
local conf = require("telescope.config").values
local telescope = require("telescope")
local actions = require "telescope.actions"
local action_state = require "telescope.actions.state"
local action_utils = require "telescope.actions.utils"
local previewers = require "telescope.previewers"
local entry_display = require("telescope.pickers.entry_display")
local make_entry = require "telescope.make_entry"
local strings = require "plenary.strings"
local Job = require('plenary.job')

local M = {}

M._colors = {
  Solution = {
    None = nil,
    Loading = { fg = "#fabd2f" },
    Done = { fg = "#b8bb26" },
  },
  Operation = {
    None = nil,
    Running = { fg = "#fabd2f" },
    Done = { fg = "#b8bb26" },
    Failed = { fg = "#fb4934" }
  }
}

M._state = {
	SolutionLoadingState = nil,
	Requests = {},
	SolutionLoaded = false,
	NumberOfProjects = 0,
	NumberOfFailedProjects = 0,
	NumberOfProjectsLoaded = 0,
	SolutionName = '',
	AssembliesLoaded = false,
	NextSequence = 1001,
	StartSent = false,
  NugetSources = {{
    Source = 'https://api.nuget.org/v3/index.json'
  },}
}

M.OpenLog = function()
  local outfile = string.format("%s/%s.log", vim.api.nvim_call_function("stdpath", { "cache" }), M.log.plugin)
	local vimScriptCommand = 'e ' .. M._state.LogFilePath
	vim.cmd(vimScriptCommand)
end

M.ClearLog = function()
  local outfile = string.format("%s/%s.log", vim.api.nvim_call_function("stdpath", { "cache" }), M.log.plugin)
  local cmd = '!del ' .. M._state.LogFilePath
  vim.cmd(cmd)
end

M._tableContains = function(t, val)
	for _, v in pairs(t) do
		if v == val then return true end
	end
	return false
end

M.SetNugetSources = function(sources)
  M._state.NugetSources = sources
end

M.AddNugetSource = function(source)
  if M._tableContains(M._state.NugetSources, source) then
    M.log.Debug('nuget source already added skipping :' .. source)
  else
    table.insert(M._state.NugetSources, source)
  end
end

M._checkNotRunning = function()
  local result = M._state.StartSent == false
  if result then
    print 'Decompiler not running.  Skipping operation'
  end
  return result
end

M.StartDecompiler = function()
	if M._state.StartSent then
		print 'Decompiler has already been started'
  elseif vim.fn.expand('%:e') == 'sln' then
    M.Start(vim.fn.expand('%'))
	else
		local dir = vim.fn.fnamemodify(vim.fn.expand('%'), ':p:h')
		local slnFiles = {}
		M._findSolutions(dir, slnFiles)
		if next(slnFiles) == nil then
			print('No solution file found')
		elseif table.getn(slnFiles) == 1 then
			M.Start(slnFiles[1])
		else
			M._openSolutionTelescope(slnFiles, M._createSolutionFileDisplayer)
		end
	end
end

M._findSolutions = function(dir, resultFiles)
	local slnFiles = vim.fn.split(vim.fn.globpath(dir, '*.sln'), '\n')
	if next(slnFiles) ~= nil then
		for k, v in ipairs(slnFiles) do
			local fullFileName = vim.fn.fnamemodify(v, ':p')
			table.insert(resultFiles, fullFileName)
		end
	else
		local parentDir = vim.fn.fnamemodify(dir, ':p:h:h')
		if dir == parentDir then
			print('Not found, hit root ' .. parentDir)
		else
			M._findSolutions(parentDir, resultFiles)
		end
	end
end

M._createSolutionFileDisplayer = function()
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ remaining = true },
			}
		}

		local ordinal = entry.value
		local make_display = function(entry)
			return displayer {
				{ entry, "TelescopeResultsIdentifier" },
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = ordinal
		}
	end
	return resultFunc
end

M._openSolutionTelescope = function(data)
	opts = opts or {}
	pickers.new(opts, {
		layout_strategy='vertical',
		prompt_title = "Select Solution File",
		finder = finders.new_table {
			results = data,
		},
		attach_mappings = function(prompt_bufnr, map)
			actions.select_default:replace(function()
				local selection = action_state.get_selected_entry()
				M.Start(selection.value)
				actions.close(prompt_bufnr)
			end)
			return true
		end,
		sorter = conf.generic_sorter(opts),
	}):find()
end

M.GetOperationStatusColor = function()
  if M._state.CurrentCommand == nil then
    return M._colors.Operation.None
  elseif M._state.CurrentCommand.Status == 'Running' then
    return M._colors.Operation.Running
  elseif M._state.CurrentCommand.Status == 'Done' then
    return M._colors.Operation.Done
  elseif M._state.CurrentCommand.Status == 'Failed' then
    return M._colors.Operation.Failed
  end
end

M.GetSolutionLoadingColor = function()
  if M._state.SolutionLoadingState == nil then
    return M._colors.Solution.None
  elseif M._state.SolutionLoadingState == 'loading' then
    return M._colors.Solution.Loading
  elseif M._state.SolutionLoadingState == 'done' then
    return M._colors.Solution.Done
  end
end

M.GetCurrentOperationMessage = function()
	local currentCommand = M._state.CurrentCommand
	if currentCommand == nil then
		return "0#: No pending operations"
	end
	local duration = currentCommand.Duration
	if duration == nil then
		duration = os.difftime(os.time(), currentCommand.StartTime)
	end
	local result = '0#: ' .. currentCommand.Status .. ' ' .. currentCommand.Name .. ' ' .. duration .. 's'
	return result
end

M.GetSolutionLoadingStatus = function()
	local statusString = ''
	local numberOfProjectsString = '(' ..  M._state.NumberOfProjectsLoaded .. ' of ' .. M._state.NumberOfProjects .. ')'
	if M._state.NumberOfFailedProjects ~= 0 then
		numberOfProjectsString = numberOfProjectsString .. ' (' .. M._state.NumberOfFailedProjects .. ' failed)'
	end
	if M._state.SolutionLoadingState == nil then
		statusString = "Not running"
	elseif M._state.SolutionLoadingState == "loading" then
		statusString = M._state.SolutionName .. ' loading... ' .. numberOfProjectsString
	elseif M._state.SolutionLoadingState == "done" then
		statusString = M._state.SolutionName .. ' ' .. numberOfProjectsString
	end
	local result = 'O#: ' .. statusString
	return result
end

local on_output = function(err, data)
  vim.schedule(function()
		M.log.debug(data)
	end)
	local ok, json = pcall(
		vim.json.decode,
		data	
	)

	if ok == true then
		local messageType = json["Event"]
		if messageType == "SOLUTION_PARSED" then
			M._state.NumberOfProjects	= json.Body.NumberOfProjects
			M._state.SolutionName = json.Body.SolutionName
		end
		if messageType == "PROJECT_LOADED" then
			M._state.NumberOfProjectsLoaded = M._state.NumberOfProjectsLoaded + 1
		end
		if messageType == "PROJECT_FAILED" then
			M._state.NumberOfFailedProjects = M._state.NumberOfFailedProjects + 1
		end
		if messageType == "ASSEMBLIES_LOADED" then
			M._state.AssembliesLoaded = true
			if M._state.SolutionLoadingState ~= 'done' and
				M._state.NumberOfProjects > 0 and
				M._state.NumberOfProjectsLoaded + M._state.NumberOfFailedProjects == M._state.NumberOfProjects and
				M._state.AssembliesLoaded == true then
				M._state.SolutionLoadingState = "done"
			end
		end
		local mType = json["Type"]
		if mType == "event" then
			if messageType == "log" then
				if json.Body.LogLevel == 'Error' then
					M.log.Error(data)
				end
			end
		elseif mType == 'response' then
			local commandState = M._state.Requests[json.Request_seq]
			M._state.Requests[json.Request_seq] = nil
			local duration = os.difftime(os.time(), M._state.CommandStartTime)
			commandState.Duration = duration
			if json.Success then
				commandState.Status = "Done"
				local commandCallback = commandState.Callback
				local startTime = commandState.StartTime
				local data = commandState.Data
				vim.schedule(function()
          commandCallback(json, data)
        end)
			else
				commandState.Status = 'Failed'
			end
		end
	end
end

M.Start = function (solutionPath)
	local pluginRootDir = vim.fn.fnamemodify(debug.getinfo(1).source:sub(2), ":h:h:h")
	M._state['StartSent'] = true
	if solutionPath == nil then
		solutionPath = vim.fn.expand('%:p')
	end
	local job = Job:new({
		command = pluginRootDir .. '\\StdIoHost\\bin\\Debug\\csdecompile.exe',
		args = {  solutionPath },
		cwd = '.',
		on_stdout = on_output,
		on_exit = function(j, return_val)
		end,
	})
	M._state.SolutionLoadingState = 'loading'

	job:start()

	M._state["job"] = job
end

M.StartNoSolution = function ()
	local pluginRootDir = vim.fn.fnamemodify(debug.getinfo(1).source:sub(2), ":h:h:h")
	M._state['StartSent'] = true
	local job = Job:new({
		command = pluginRootDir .. '\\StdIoHost\\bin\\Debug\\csdecompile.exe',
		args = { "--nosolution" },
		cwd = '.',
		on_stdout = on_output,
		on_exit = function(j, return_val)
		end,
	})
	M._state.SolutionLoadingState = 'loading'

	job:start()
	M._state.SolutionName = 'Started (No Solution)'
	M._state.SolutionLoadingState = 'done'

	M._state["job"] = job
end

M.StartFromMemoryDump = function (memoryDumpPath)
	local pluginRootDir = vim.fn.fnamemodify(debug.getinfo(1).source:sub(2), ":h:h:h")
	M._state['StartSent'] = true
	local job = Job:new({
		command = pluginRootDir .. '\\StdIoHost\\bin\\Debug\\csdecompile.exe',
		args = { "--memorydump", memoryDumpPath },
		cwd = '.',
		on_stdout = on_output,
		on_exit = function(j, return_val)
		end,
	})
	M._state.SolutionLoadingState = 'loading'

	job:start()
	M._state.SolutionName = 'Started (memory dump)'
	M._state.SolutionLoadingState = 'done'

	M._state["job"] = job
end

M.StartFromProcess = function (processId)
	local pluginRootDir = vim.fn.fnamemodify(debug.getinfo(1).source:sub(2), ":h:h:h")
	M._state['StartSent'] = true
	local job = Job:new({
		command = pluginRootDir .. '\\StdIoHost\\bin\\Debug\\csdecompile.exe',
		args = { "--process", processId },
		cwd = '.',
		on_stdout = on_output,
		on_exit = function(j, return_val)
		end,
	})
	M._state.SolutionLoadingState = 'loading'

	job:start()
	M._state.SolutionName = 'Started (process)'
	M._state.SolutionLoadingState = 'done'

	M._state["job"] = job
end

M._sendStdIoRequest = function(request, callback, callbackData)
  if M._state.Requests[M._state.NextSequence + 1] then
    M._state.Requests[M._state.NextSequence + 1] = nil
  end

	local nextSequence = M._state.NextSequence + 1

	local command = { Callback = callback, StartTime = os.time(), Data = callbackData, Name = request.Command, Status = 'Running' }
	M._state.Requests[nextSequence] = command

	M._state.NextSequence = nextSequence
	request["Seq"] = nextSequence
	local requestJson = vim.json.encode(request) .. '\n'
  M.log.debug(requestJson)
	M._state.CurrentCommand = command
	M._state.job.stdin:write(requestJson)
	M._state.CommandStartTime = os.time()
	M._state.EndTime = nil
	M._state.CurrentSeq = nextSequence
end

M._getStartOfCurrentWord = function()
 local curLine = vim.api.nvim_get_current_line()
 local currentCol = vim.api.nvim_win_get_cursor(0)[2]

 local result = 0
 while(currentCol > 0)
 do
   local curChar = string.sub(curLine, currentCol + 1 , currentCol + 1)
   if string.match(curChar, "[%w_@]") then
     currentCol = currentCol - 1
     result = currentCol + 2
   else
     break
   end
 end

 return result
end

M._createDecompileRequest = function(url, callback, callbackData)
	local cursorPos = vim.api.nvim_win_get_cursor(0)
	local line = cursorPos[1]
	local column = cursorPos[2] + 1
  column = M._getStartOfCurrentWord()
	local assemblyFilePath = vim.b.AssemblyFilePath
	local parentAssemblyFilePath = vim.b.parentAssemblyFilePath
	local assemblyName = vim.b.AssemblyName
	local fileName = vim.fn.expand('%:p')

	local locationType = vim.b.Type
	if locationType == nil then
		locationType = 1
	end

	local request = {
		Command = url,
		Arguments = {
			FileName = fileName,
			AssemblyFilePath = vim.b.AssemblyFilePath,
			ParentAssemblyFilePath = vim.b.ParentAssemblyFilePath,
			AssemblyName = vim.b.AssemblyName,
			Type = locationType,
			ContainingTypeFullName = vim.b.ContainingTypeFullName,
			Column = column,
			Line = line,
		},
	}
  return request
end

M._decompileRequest = function(url, callback, callbackData)
  local request = M._createDecompileRequest(url, callback, callbackData)
	M._sendStdIoRequest(request, callback, callbackData)
end

M.StartAddExternalDirectory = function(directoryFilePath)
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/addexternalassemblydirectory",
		Arguments = {
			DirectoryFilePath = directoryFilePath
		}
	}
	M._sendStdIoRequest(request, M.HandleAddExternalDirectory);
end

M.HandleAddExternalDirectory = function(response)
end

M.StartUniqCallStacks = function()
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/uniqCallStacks",
		Arguments = { Stub = true }
	}
	M._sendStdIoRequest(request, M.HandleUniqCallStacks);
end

M.HandleUniqCallStacks = function(response)
  local threads = {}
  for _, item in ipairs(response.Body.Result) do
    local frames = {}
    local result = ""
    for _, thread in ipairs(item.Threads) do
        result = result .. string.format("OsId: 0x%x ManagedId: 0x%x,", thread.OSId, thread.ManagedId)
    end
    for _, frame in ipairs(item.Frames) do
      table.insert(frames, { str = string.format("0x%x 0x%x 0x%x %s.%s", frame.StackPointer, frame.InstructionPointer, frame.MetadataToken, frame.TypeName, frame.MethodName), obj = frame })
    end
    -- Remove the trailing comma
    result = result:sub(1, -2)
    table.insert(threads, { threadsStr = result, threads = item.Threads, frames = frames })
  end

  M._openTelescope(threads, M._createUniqCallStackDisplayer, M._uniqCallStackPreviewer, function(selection)
    local captureFrames = selection.frames
    M._openTelescope(selection.threads, M._createUniqCallStackThreadDisplayer, nil, function(selection)
      local captureThread = selection
      M._openTelescope(captureFrames, M._createUniqCallStackThreadFramesDisplayer, nil, function(selection)
        M.StartDecompileFrame(selection.obj.StackPointer, { Entry = value, BufferNumber = 0, WindowId = 0, })
      end,
      'Uniq Call Stack Frames')
    end,
    'Uniq Call Stack Threads')
  end,
  'Uniq Call Stacks')
end

M.StartAddMemoryDumpAssemblies = function(memoryDumpFilePath)
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/addmemorydumpassemblies",
		Arguments = {
			MemoryDumpFilePath = memoryDumpFilePath
		}
	}
	M._sendStdIoRequest(request, M.HandleAddMemoryDumpAssemblies);
end

M.HandleAddMemoryDumpAssemblies = function(response)
end

M.StartGetAssemblies = function()
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/getassemblies",
		Arguments = {
			Load = true,
		}
	}
	M._sendStdIoRequest(request, M.HandleGetAssemblies);
end

M.StartGetAssembliesForSearchMembers = function(memberSearchString)
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/getassemblies",
		Arguments = {
			Load = true,
		}
	}
	M._sendStdIoRequest(request, M.HandleGetAssembliesForSearchMembers, { MemberSearchString = memberSearchString });
end

M.HandleGetAssembliesForSearchMembers = function(response, state)
	M._openAssembliesTelescopeMultiSelect(response.Body.Assemblies, M.StartSearchMembersFromTelescope, state)
end

M.StartSearchMembersFromTelescope = function(selections, state)
  local assemblyNames = {}
  for i,v in ipairs(selections) do
    table.insert(assemblyNames, v.value.FullName)
  end

  M.StartSearchMembers(assemblyNames, state.MemberSearchString)
end

M.StartGetAssembliesForDecompile = function()
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/getassemblies",
		Arguments = {
			Load = true,
		}
	}
	M._sendStdIoRequest(request, M.HandleGetAssembliesForDecompile);
end

M.HandleGetAssemblies = function(response)
	M._openAssembliesTelescope(response.Body.Assemblies, M.StartGetAssemblyTypes)
end

M.HandleGetAssembliesForDecompile = function(response)
	M._openAssembliesTelescope(response.Body.Assemblies, M.StartDecompileAssembly)
end

M.StartGetAssemblyTypes = function(filePath, assemblyName)
	local request = {
		Command = "/getassemblytypes",
		Arguments = {
			AssemblyFilePath = filePath,
		}
	}
	M._sendStdIoRequest(request, M.HandleGetAllTypes)
end

M.StartDecompileAssembly = function(filePath, assemblyName)
	local request = {
		Command = "/decompileassembly",
		Arguments = {
			AssemblyFilePath = filePath,
			AssemblyName = assemblyName
		}
	}
	M._sendStdIoRequest(request, M.HandleDecompileGotoDefinitionResponse)
end

M._openAssembliesTelescope = function(data, resultHandler, state)
  local base = M._openAssembliesTelescopeBase(data)
  base.attach_mappings = function(prompt_bufnr, map)
    actions.select_default:replace(function()
      local selection = action_state.get_selected_entry()

      action_utils.map_selections(prompt_bufnr, function(entry, index)
        print(vim.inspect(entry))
      end)

      actions.close(prompt_bufnr)
      resultHandler(selection.value.FilePath, selection.value.FullName, state)
    end)
    return true
  end
  base:find()
end

M._openAssembliesTelescopeMultiSelect = function(data, resultHandler, state)
  local base = M._openAssembliesTelescopeBase(data)
  base.attach_mappings = function(prompt_bufnr, map)
    actions.select_default:replace(function()
      local selection = action_state.get_selected_entry()

      local multiSelections = {}
      action_utils.map_selections(prompt_bufnr, function(entry, index)
        table.insert(multiSelections, entry)
      end)

      actions.close(prompt_bufnr)
      if next(multiSelections) == nil then
        resultHandler({selection}, state)
      else
        resultHandler(multiSelections, state)
      end
    end)
    return true
  end
  base:find()
end

M._openAssembliesTelescopeBase = function(data)
	local widths = {
		FullName = 0,
		TargetFrameworkId = 0,
	}

	local parse_line = function(entry)
		for key, value in pairs(widths) do
			widths[key] = math.max(value, strings.strdisplaywidth(entry[key] or ""))
		end
	end

	for _, line in ipairs(data) do
		parse_line(line)
	end

	opts = opts or {}
  local result = pickers.new(opts, {
    layout_strategy='vertical',
    prompt_title = "Assemblies",
    finder = finders.new_table {
      results = data,
      entry_maker = function(entry)
        local displayer = entry_display.create {
          separator = "  ",
          items = {
            { width = widths.FullName },
            { remaining = true },
          },
        }
        local make_display = function(entry)
          return displayer {
            { M._blankIfNil(entry.value.FullName), "TelescopeResultsIdentifier" },
            { M._blankIfNil(entry.value.TargetFrameworkId), "TelescopeResultsClass" },
          }
        end
        return {
          value = entry,
          display = make_display,
          ordinal = entry.FullName
        }
      end
    },
    attach_mappings = function(prompt_bufnr, map)
      actions.select_default:replace(onSelection)
      -- actions.select_default:replace(function()
      --   local selection = action_state.get_selected_entry()

      --   local multiSelections = {}
      --   action_utils.map_selections(prompt_bufnr, function(entry, index)
      --     table.insert(multiSelections, entry)
      --   end)

      --   actions.close(prompt_bufnr)
      --   if next(multiSelections) == nil then
      --     resultHandler({selection}, state)
      --   else
      --     resultHandler(multiSelections, state)
      --   end
      -- end)
      return true
    end,
    sorter = conf.generic_sorter(opts),
  })
  return result;
end

M.ClearNugetDirectory = function()
  local cmd = '!rd /s /q ' .. M._state.PackageDirectory
  vim.cmd(cmd)
end

M.StartFindMethodByFilePath = function()
  local lineText = vim.api.nvim_get_current_line()
  local path_start, path_end = lineText:find("in ")
  if path_start and path_end then
    local line_start, line_end = lineText:find(":line ", path_end)
    if line_start and line_end then
      local path = lineText:sub(path_start + 3, line_start - 1)
      local line = lineText:sub(line_end + 1)
      line = line:match("%d+")
      vim.cmd('e ' .. path)
      vim.cmd(line)
    end
  end
end

M.StartFindMethodByName = function(namespaceName, typeName, methodName)
  if M._checkNotRunning() then
    return
  end

  local request = {
    Command = "/findmethodbyname",
    Arguments = {
      NamespaceName = namespaceName,
      TypeName = typeName,
      MethodName = methodName
    }
  }
  M._sendStdIoRequest(request, M.HandleGetTypeMembers);
end

M.StartFindMethodByStackFrame = function()
  if M._checkNotRunning() then
    return
  end

  stackFrame = vim.call('expand','<cWORD>')

  local request = {
    Command = "/findmethodbystackframe",
    Arguments = {
      StackFrame = stackFrame
    }
  }
  M._sendStdIoRequest(request, M.HandleGetTypeMembers);
end

M.HandleGetNamespaces = function()
end

M.StartSearchNuget = function(searchString, downloadDependencies)
  if M._checkNotRunning() then
    return
  end

  local request = {
    Command = "/searchnuget",
    Arguments = {
      SearchString = searchString,
      NugetSources = M._state.NugetSources
    }
  }
  M._sendStdIoRequest(request, M.HandleSearchNuget, { DownloadDependencies = downloadDependencies });
end

M.StartSearchNugetFromLocation = function()
  if M._checkNotRunning() then
    return
  end

  local request = M._createDecompileRequest("/searchnugetfromlocation")
  request.Arguments.NugetSources = M._state.NugetSources
  M._sendStdIoRequest(request, M.HandleSearchNugetFromLocation)
end

M.StartGetAllTypes = function(searchString)
  if M._checkNotRunning() then
    return
  end
	local request = {
		Command = "/gettypes",
		Arguments = {
			SearchString = searchString
		}
	}
	M._sendStdIoRequest(request, M.HandleGetAllTypes);
end

M.HandleSearchNugetFromLocation = function(response)
  M._openTelescope(response.Body.Packages, M._createSearchNugetDisplayer, nil, function(selection)
    local request = {
      Command = "/getnugetpackageversions",
      Arguments = {
        NugetSources = response.Body.NugetSources,
        PackageId = selection.PackageId,
        ParentAssemblyMajorVersion = response.Body.ParentAssemblyMajorVersion,
        ParentAssemblyMinorVersion = response.Body.ParentAssemblyMinorVersion,
        ParentAssemblyBuildVersion = response.Body.ParentAssemblyBuildVersion
      }
    }
    M._sendStdIoRequest(request, M.HandleGetNugetPackageVersionsFromLocation);
  end,
  'Search Nuget: ' .. response.Body.SearchString)
end

M.HandleSearchNuget = function(response, state)
  M._openTelescope(response.Body.Packages, M._createSearchNugetDisplayer, nil, function(selection)
    local request = {
      Command = "/getnugetpackageversions",
      Arguments = {
        NugetSources = response.Body.NugetSources,
        PackageId = selection.PackageId
      }
    }
    M._sendStdIoRequest(request, M.HandleGetNugetPackageVersions, state);
  end,
  'Search Nuget: ' .. response.Body.SearchString)
end

M._sortPackagesByVersion = function(packages)
  table.sort(packages, function(a, b) 
    if a.MajorVersion == b.MajorVersion then
      if a.MinorVersion == b.MinorVersion then
        if a.Build == b.Build then
          return a.Revision > b.Revision
        end
        return a.Patch > b.Patch
      end
      return a.MinorVersion > b.MinorVersion
    end
    return a.MajorVersion > b.MajorVersion
  end)
end

M.HandleGetNugetPackageVersions = function(response, state)
  M._sortPackagesByVersion(response.Body.Packages)

  M._openTelescope(response.Body.Packages, M._createGetNugetVersionsDisplayer, nil, function(selection)
    if state.DownloadDependencies then
      local request = {
        Command = "/getnugetpackagedependencygroups",
        Arguments = {
          NugetSources = response.Body.NugetSources,
          PackageId = selection.PackageId,
          PackageVersion = selection.PackageVersion
        }
      }
      M._sendStdIoRequest(request, M.HandleGetNugetPackageDependencyGroups, {
        PackageId = selection.PackageId,
        PackageVersion = selection.PackageVersion,
        DownloadDependencies = state.DownloadDependencies
      });
    else
      local request = {
        Command = '/addnugetpackage',
        Arguments = {
          NugetSources = response.Body.NugetSources,
          PackageId = selection.PackageId,
          PackageVersion = selection.PackageVersion,
          RootPackageDirectory = M._state.PackageDirectory
        }
      }
      M._sendStdIoRequest(request, M.HandleAddNugetPackageAndDependencies);
    end
  end,
  'Nuget Versions: ' .. response.Body.PackageId)
end

M.HandleGetNugetPackageVersionsFromLocation = function(response)
  M._sortPackagesByVersion(response.Body.Packages)

  local versionSearchString = response.Body.ParentAssemblyMajorVersion .. '.' .. response.Body.ParentAssemblyMinorVersion .. '.' .. response.Body.ParentAssemblyBuildVersion
  M._openTelescope(response.Body.Packages, M._createGetNugetVersionsDisplayer, nil, function(selection)
    local request = {
      Command = "/getnugetpackagedependencygroups",
      Arguments = {
        NugetSources = response.Body.NugetSources,
        PackageId = selection.PackageId,
        PackageVersion = selection.PackageVersion
      }
    }
    M._sendStdIoRequest(request, M.HandleGetNugetPackageDependencyGroups, {
      PackageId = selection.PackageId,
      PackageVersion = selection.PackageVersion,
      DownloadDependencies = true
    });
  end,
  'Nuget Versions: ' .. response.Body.PackageId,
  versionSearchString)
end

M.HandleGetNugetPackageDependencyGroups = function(response, data)
  M._openTelescope(response.Body.Groups, M._createNugetDependencyGroupDisplayer, nil, function(selection)
    local command
    if data.DownloadDependencies then
      command = "/addnugetpackageanddependencies"
    else
      command = "/addnugetpackage"
    end

    local request = {
      Command = command,
      Arguments = {
        NugetSources = response.Body.NugetSources,
        PackageId = data.PackageId,
        PackageVersion = data.PackageVersion,
        DependencyGroup = selection,
        RootPackageDirectory = M._state.PackageDirectory
      }
    }
    M._sendStdIoRequest(request, M.HandleAddNugetPackageAndDependencies);
  end,
  'Dependency Group: ' .. response.Body.PackageId .. ' ' .. response.Body.PackageVersion)
end

M.HandleAddNugetPackageAndDependencies = function(response, data)
end

M.HandleGetAllTypes = function(response)
  M._openTelescope(response.Body.Locations, M._createGetAllTypesDisplayer, M._sourcePreviewer, function(selection)
    local current_buffer = vim.fn.bufnr()
    local variable_exists = vim.b.isSymoblWindow ~= nil

    if variable_exists then
      local winid = vim.api.nvim_get_current_win()
      vim.api.nvim_win_close(winid, true)
    end
    M._openSourceFileOrDecompile(selection)
  end,
  'Search Types')
end

M.StartDecompileGotoDefinition = function()
  if M._checkNotRunning() then
    return
  end

  local current_buffer = vim.fn.bufnr()
  local variable_exists = vim.b.isSymoblWindow ~= nil

  if variable_exists then
    local curword = vim.fn.expand('<cword>')
    M.StartGetAllTypes(curword)
  else
    M._decompileRequest('/gotodefinition', M.HandleFindImplementations)
  end
end

M.StartFindUsages = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest("/findusages", M.HandleUsages)
end

M.HandleUsages = function(response)
  M._openTelescope(response.Body.Locations, M._createUsagesDisplayer, M._sourcePreviewer, function(selection)
      M._openSourceFileOrDecompile(selection)
  end,
  'Find Usages')
end

M.StartGetSymbolName = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest("/symbolinfo", M.HandleGetSymbolName)
end

M.HandleGetSymbolName = function(response)
  M._navigationFloatingWin(response.Body)
end

M.StartRefreshBuffer = function()
	local cursorPos = vim.api.nvim_win_get_cursor(0)
	local line = cursorPos[1]
	local column = cursorPos[2] + 1

	local request = {
		Command = "/decompiledsource",
		Arguments = {
			AssemblyFilePath = vim.b.AssemblyFilePath,
			ContainingTypeFullName = vim.b.ContainingTypeFullName,
			Line = line,
			Column = column,
		},
		Seq = M._state.NextSequence,
	}

  local bufnr = vim.fn.bufnr()
  local winid = vim.api.nvim_get_current_win()
	M._sendStdIoRequest(request, M.HandleDecompiledSource, { Entry = nil, BufferNumber = bufnr, WindowId = winid, })
end

M.StartDecompileFrame = function(stackPointer, callbackData)
	local request = {
		Command = "/decompileframe",
		Arguments = {
      StackPointer = stackPointer,
		},
		Seq = M._state.NextSequence,
	}

	M._sendStdIoRequest(request, M.HandleDecompiledSource, callbackData)
end

M.StartGetDecompiledSource = function(
  parentAssemblyFilePath,
	assemblyFilePath,
	containingTypeFullName,
	line,
	column,
	callbackData)

	local request = {
		Command = "/decompiledsource",
		Arguments = {
      ParentAssemblyFilePath = parentAssemblyFilePath,
			AssemblyFilePath = assemblyFilePath,
			ContainingTypeFullName = containingTypeFullName,
			Line = line,
			Column = column,
		},
		Seq = M._state.NextSequence,
	}

	M._sendStdIoRequest(request, M.HandleDecompiledSource, callbackData)
end

M.StartSearchMembers = function(assemblySearchStrings, memberSearchString)
  if M._checkNotRunning() then
    return
  end

  local request = {
    Command = "/searchmembers",
    Arguments = {
      AssemblySearchStrings = assemblySearchStrings,
      MemberSearchString = memberSearchString
    }
  }
	M._sendStdIoRequest(request, M.HandleSearchMembers)
end

M.HandleSearchMembers = function(response)
  M._openTelescope(response.Body.Locations, M._createUsagesDisplayer, M._sourcePreviewer, function(selection)
      M._openSourceFileOrDecompile(selection)
  end,
  'Members')
end

M.StartGetTypeMembers = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest('/gettypemembers', M.HandleGetTypeMembers)
end

M.HandleGetTypeMembers = function(response)
  M._openTelescope(response.Body.Locations, M._createUsagesDisplayer, M._sourcePreviewer, function(selection)
    M._openSourceFileOrDecompile(selection)
  end,
  'Type Members')
end

M.StartFindImplementations = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest('/findimplementations', M.HandleFindImplementations)
end

M.HandleFindImplementations = function(response)
  if response.Body == vim.NIL then
    print 'No implementations found'
  elseif response.Body == nil then
    print 'No implementations found'
  elseif #response.Body.Locations == 0 then
    print 'No implementations found'
  elseif #response.Body.Locations == 1 then
    M._openSourceFileOrDecompile(response.Body.Locations[1])
  else
    M._openTelescope(response.Body.Locations, M._createUsagesDisplayer, M._sourcePreviewer, function(selection)
      M._openSourceFileOrDecompile(selection)
    end,
    'Find Implementations')
  end
end

M._openSourceFileOrDecompile = function(value)
	if value.Type == 1 then
		local bufnr = vim.uri_to_bufnr(value.FileName)
		vim.api.nvim_win_set_buf(0, bufnr)
		vim.api.nvim_win_set_cursor(0, { value.Line, value.Column })
	else
		M.StartGetDecompiledSource(
      value.ParentAssemblyFilePath,
			value.AssemblyFilePath,
			value.ContainingTypeFullName,
			value.Line,
			value.Column,
			{ Entry = value, BufferNumber = 0, WindowId = 0, })
	end
end

M.HandleDecompiledSource = function(response, data)
	local body = response.Body
	local location = body.Location
	local bufnr = data.BufferNumber
	local winid = data.WindowId

	if response.Request_seq == M._state.CurrentSeq then
		M._setBufferTextFromDecompiledSource(location, body.SourceText, bufnr, winid)
	end
end

M.HandleDecompileGotoDefinitionResponse = function(response)
	local body = response.Body
  local location = body.Location

  if location.Type == 1 then
    local location = body.Location
    local fileName = location.FileName
		M._openSourceFile(location)
	else
    local fileName = location.FileName
		if response.Request_seq == M._state.CurrentSeq then
			M._setBufferTextFromDecompiledSource(location, body.SourceText, 0, 0)
		end
	end
end

M._setCursorFromLocation = function(winid, location)
	local column = 0
	local line = location.Line

	if location.Column ~= 0 then
		column = location.Column - 1
	end

	vim.api.nvim_win_set_cursor(winid, { line, column })
end

M._openSourceFile = function(location)
		local bufnr = vim.uri_to_bufnr(location.FileName)
		vim.api.nvim_win_set_buf(0, bufnr)
		vim.api.nvim_buf_set_option(bufnr, "buflisted", true)
		M._setCursorFromLocation(0, location)
end

M._setBufferTextFromDecompiledSource = function(location, sourceText, bufnr, winid)
	local lines = {}
	local decompileFileName = ''
	local fileName = location.FileName
	local column = location.Column - 1

		if bufnr == 0 then
			if location.Type == 0 then
				decompileFileName = location.ContainingTypeFullName .. '.cs'
			elseif location.Type == 2 then
				decompileFileName = location.AssemblyName .. '.cs'
			end
			bufnr = vim.uri_to_bufnr("c:\\TEMP\\DECOMPILED_" .. decompileFileName)
		end
		vim.list_extend(lines, vim.split(sourceText, "\r\n"))
		vim.api.nvim_buf_set_lines(bufnr, 0, -1, false, lines)
		vim.api.nvim_win_set_buf(winid, bufnr)
		M._setCursorFromLocation(winid, location)
		vim.api.nvim_buf_set_option(bufnr, "syntax", "cs")
		vim.api.nvim_buf_set_option(bufnr, "buftype", "nofile")
		vim.api.nvim_buf_set_option(bufnr, "buflisted", true)
		vim.api.nvim_buf_add_highlight(bufnr, -1, "TelescopePreviewLine", location.Line -1, 0, -1)
		vim.b.Type = location.Type
		vim.b.AssemblyFilePath = location.AssemblyFilePath
		vim.b.ParentAssemblyFilePath = location.ParentAssemblyFilePath
		vim.b.ContainingTypeFullName = location.ContainingTypeFullName
end

M._blankIfNil = function(val)
	local result = ''
  if val ~= nil then
    if type(val) == "string" then
      return val
    else
      return ''
    end
  end
	return ''
end

M._createGetAllTypesDisplayer = function(widths)
	local resultFunc = function(entry)
		local make_display = nil
		if entry.Type == 0 then
			local displayer = entry_display.create {
				separator = "  ",
				items = {
          { width = 2 },
					{ width = widths.ContainingTypeFullName },
					{ width = widths.NamespaceName },
					{ width = widths.AssemblyName + widths.AssemblyVersion + 1 },
					{ width = widths.DotNetVersion + 6 },
					{ remaining = true },
				},
			}

			make_display = function(entry)
				return displayer {
          { "" },
					{ M._blankIfNil(entry.value.ContainingTypeFullName), "TelescopeResultsClass" },
					{ M._blankIfNil(entry.value.NamespaceName), "TelescopeResultsIdentifier" },
					{ string.format("%s %s", entry.value.AssemblyName, entry.value.AssemblyVersion), "TelescopeResultsIdentifier" },
					{ string.format("%s %s", '.net ', entry.value.DotNetVersion), "TelescopeResultsIdentifier" },
					{ M._blankIfNil(entry.value.AssemblyFilePath), "TelescopeResultsIdentifier" }
				}
			end
		else
			local displayer = entry_display.create {
				separator = "  ",
				items = {
          { width = 2 },
					{ width = widths.ContainingTypeFullName },
					{ width = widths.NamespaceName },
					{ remaining = true },
				},
			}

			make_display = function(entry)
				return displayer {
          { "" },
					{ M._blankIfNil(entry.value.ContainingTypeFullName), "TelescopeResultsClass" },
					{ M._blankIfNil(entry.value.NamespaceName), "TelescopeResultsIdentifier" },
					{ string.format("%s:%s:%s", entry.value.FileName, entry.value.Line, entry.value.Column), "TelescopeResultsIdentifier" }
				}
			end
		end

		return {
			value = entry,
			display = make_display,
			ordinal = entry.SourceText
		}
	end

	return resultFunc
end

M._createUniqCallStackDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(entry.value.threadsStr), "TelescopeResultsClass" },
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = entry.threadsStr
		}
	end
	return resultFunc
end

M._createUniqCallStackThreadFramesDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(entry.value.str), "TelescopeResultsClass" },
			}
		end

    print(vim.inspect(entry))
		return {
			value = entry,
			display = make_display,
			ordinal = tostring(entry.obj.Ordinal)
		}
	end
	return resultFunc
end

M._createUniqCallStackThreadDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(string.format("OsId: 0x%x ManagedId: 0x%x", entry.value.OSId, entry.value.ManagedId)), "TelescopeResultsClass" },
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = string.format("%s", entry.OSId)
		}
	end
	return resultFunc
end

M._createSearchNugetDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(entry.value.PackageId), "TelescopeResultsClass" },
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = entry.PackageId
		}
	end
	return resultFunc
end

M._createNugetDependencyGroupDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(entry.value), "TelescopeResultsClass" },
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = entry
		}
	end
	return resultFunc
end

M._createGetNugetVersionsDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
        { width = widths.PackageId },
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(entry.value.PackageId), "TelescopeResultsClass" },
				{ M._blankIfNil(entry.value.PackageVersion), "TelescopeResultsClass" },
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = entry.PackageVersion
		}
	end
	return resultFunc
end

M._createUsagesDisplayer = function(widths)
	local resultFunc = function(entry)
		local make_display = nil
		local ordinal = nil
		if entry.Type == 1 then
			local displayer = entry_display.create {
				separator = "  ",
				items = {
          { width = 2 },
					{ width = widths.ContainingTypeShortName },
					{ remaining = true },
				}
			}

			ordinal = M._blankIfNil(entry.FileName) .. M._blankIfNil(entry.SourceText)
			make_display = function(entry)
				return displayer {
          { "" },
					{ string.format("%s", entry.value.ContainingTypeShortName), "TelescopeResultsClass" },
					{ string.format("%s", entry.value.SourceText), "TelescopeResultsIdentifier" },
				}
			end
		else
			local displayer = entry_display.create {
				separator = "  ",
				items = {
          { width = 2 },
					{ width = widths.ContainingTypeShortName },
					{ remaining = true },
					-- { remaining = true },
				},
			}

			ordinal = M._blankIfNil(entry.ContainingTypeFullName) .. M._blankIfNil(entry.SourceText)
			make_display = function(entry)
				return displayer {
          { "" },
					{ string.format("%s", entry.value.ContainingTypeShortName), "TelescopeResultsClass" },
					{ M._blankIfNil(entry.value.SourceText), "TelescopeResultsIdentifier" },
				}
			end
		end

		return {
			value = entry,
			display = make_display,
			ordinal = ordinal
		}
	end
	return resultFunc
end


M._navigationFloatingWin = function(data)
  local buf = vim.api.nvim_create_buf(false, true)
	vim.api.nvim_buf_set_option(buf, "bufhidden", "delete")
	vim.api.nvim_buf_set_option(buf, "filetype", "cs")

  local buffer = vim.fn.bufnr()
  vim.api.nvim_buf_set_var(buf, "isSymoblWindow", "true")

	vim.api.nvim_buf_set_keymap(
		buf,
		"n",
		"<ESC>",
		'<Cmd>bd!<CR>',
		{ silent = true }
	)

	local toDisplay = {}
	local headerText = M._blankIfNil(data.DisplayName) .. ' (' .. M._blankIfNil(data.Kind) .. ')'
	table.insert(toDisplay, headerText)
	table.insert(toDisplay, '')

	for key, value in pairs(data.Properties) do
		if (type(value) == "table") then
			table.insert(toDisplay, key)
			for innerKey, innerValue in pairs(value) do
				local l = '- ' .. innerKey .. ': ' .. M._blankIfNil(innerValue)
				table.insert(toDisplay, l)
			end
		else
			local l = M._blankIfNil(key) .. ': ' .. M._blankIfNil(value)
			table.insert(toDisplay, l)
		end
	end

  table.insert(toDisplay, '')
	table.insert(toDisplay, 'Assembly Full Name: ' .. M._blankIfNil(data.ParentAssemblyFullName))
	table.insert(toDisplay, 'Target Framework: ' .. M._blankIfNil(data.TargetFramework))
	table.insert(toDisplay, 'Assembly File Path: ' .. M._blankIfNil(data.FilePath))

	vim.api.nvim_buf_set_lines(buf, 0, -1, true, toDisplay)
  vim.api.nvim_buf_set_option(buf, 'buftype', 'nofile')

	local width = vim.o.columns - 4
	local height = 22
	if (vim.o.columns >= 125) then
			width = 120
	end
	local win = vim.api.nvim_open_win(
		buf,
		true,
		{
				relative = 'editor',
				style = 'minimal',
				border = 'single',
				noautocmd = true,
				width = width,
				height = height,
				col = math.min((vim.o.columns - width) / 2),
				row = math.min((vim.o.lines - height) / 2 - 1),
		}
	)

	vim.api.nvim_win_set_option(
			win,
			"winhl",
			"Normal:Normal"
	)
	vim.api.nvim_win_set_option(
			win,
			"wrap",
			true
	)
end

M._uniqCallStackPreviewer = previewers.new_buffer_previewer {
  dyn_title = function (_, entry)
    local titleResult = 'stub'
    return titleResult
  end,
  get_buffer_by_name = function(_, entry)
    return entry.value
  end,

  define_preview = function(self, entry)
    local strArray = {}
    for _, item in ipairs(entry.value.frames) do
      table.insert(strArray, item.str)
    end
    vim.api.nvim_buf_set_option(self.state.bufnr, "syntax", "cs")
    vim.api.nvim_buf_set_lines(self.state.bufnr, 0, -1, false, strArray)
  end
}

M._sourcePreviewer = previewers.new_buffer_previewer {
  dyn_title = function (_, entry)
    local titleResult = ''
    if entry.value.Type == 1 then
      titleResult = vim.fn.fnamemodify(entry.value.FileName, ':.')
    else
      titleResult = entry.value.ContainingTypeShortName
    end
    return titleResult
  end,
  get_buffer_by_name = function(_, entry)
    return entry.value
  end,
  define_preview = function(self, entry)
    if entry.value.Type == 1 then
      local bufnr = self.state.bufnr
      local winid = self.state.winid

      conf.buffer_previewer_maker(entry.value.FileName, self.state.bufnr, {
        use_ft_detect = true,
        bufname = self.state.bufname,
        winid = self.state.winid,
        callback = function(bufnr)
          local currentWinId = vim.fn.bufwinnr(bufnr)
          if currentWinId ~= -1 then
            local startColumn = entry.value.Column
            local endColumn = entry.value.Column
            vim.api.nvim_buf_add_highlight(bufnr, -1, "TelescopePreviewLine", entry.value.Line -1, 0, -1)
            vim.api.nvim_win_set_cursor(self.state.winid, { entry.value.Line, 0 })
          end
        end
      })
    else
      local bufnr = self.state.bufnr
      local winid = self.state.winid

      M.StartGetDecompiledSource(
      entry.value.ParentAssemblyFilePath,
      entry.value.AssemblyFilePath,
      entry.value.ContainingTypeFullName,
      entry.value.Line,
      entry.value.Column,
      { Entry = entry.value, BufferNumber = bufnr, WindowId = winid, })

      vim.api.nvim_buf_set_option(self.state.bufnr, "syntax", "cs")
      vim.api.nvim_buf_set_lines(self.state.bufnr, 0, -1, false, { 'Decompiling ' .. entry.value.SourceText .. '...'})
    end
  end
}

M._openTelescope = function(data, displayFunc, previewer, onSelection, promptTitle, prompt)
	local widths = {
		ContainingTypeFullName = 0,
		ContainingTypeShortName = 0,
		TypeName = 0,
		NamespaceName = 0,
		AssemblyName = 0,
		DotNetVersion = 0,
		AssemblyVersion = 0,
		Line = 0,
		Column = 0,
		FileName = 0,
		SourceText = 0,
    Identity = 0,
	}

	local parse_line = function(entry)
		for key, value in pairs(widths) do
			widths[key] = math.max(value, strings.strdisplaywidth(entry[key] or ""))
		end
	end

	for _, line in ipairs(data) do
		parse_line(line)
	end

	local entryMaker = displayFunc(widths)

	opts = {}
	pickers.new(opts, {
		layout_strategy='vertical',
		layout_config = {
			width = 0.95,
			height = 0.95,
		},
    default_text = prompt,
		prompt_title = promptTitle,
		finder = finders.new_table {
			results = data,
			entry_maker = entryMaker,
		},
		attach_mappings = function(prompt_bufnr, map)
			actions.select_default:replace(function()
				local selection = action_state.get_selected_entry()
				actions.close(prompt_bufnr)
        onSelection(selection.value)
			end)
			return true
		end,
		previewer = previewer,
		sorter = conf.generic_sorter(opts),
	}):find()
end

M.Setup = function(config)
  if not config then
    config = {}
  end

  local logLevel = "warn"
  if config.logLevel then
    logLevel = config.logLevel
  end

  if config.NugetSources then
    M._state.NugetSources = config.NugetSources
  end

  M._state.PackageDirectory = vim.F.if_nil(
    config.PackageDirectory,
    string.format("%s\\%s\\packages\\", vim.api.nvim_call_function("stdpath", { "cache" }), 'csdecompile.nuget'))

  M.log = require("plenary.log").new({
    plugin = "csdecompile",
    level = logLevel
  })

  if config.noneSolutionColor then
    M._colors.Solution.None = config.noneSolutionColor
  end

  if config.loadingSolutionColor then
    M._colors.Solution.Loading = config.loadingSolutionColor
  end

  if config.doneLoadingSolutionColor then
    M._colors.Solution.Done = config.doneLoadingSolutionColor
  end

  if config.operationNoneColor then
    M._colors.Operation.None = config.operationNoneColor
  end

  if config.operationRunningColor then
    M._colors.Operation.Running = config.operationRunningColor
  end

  if config.operationDoneColor then
    M._colors.Operation.Done = config.operationDoneColor
  end

  if config.operationFailedColor then
    M._colors.Operation.Failed = config.operationFailedColor
  end

  M._state.LogFilePath = string.format("%s\\%s.log", vim.api.nvim_call_function("stdpath", { "cache" }), M.log.plugin)

  vim.api.nvim_create_user_command(
      'AddExternalAssemblyDirectory',
      function(opts)
        M.StartAddExternalDirectory(opts.args)
      end,
      { nargs = '?', complete='dir' }
  )
  vim.api.nvim_create_user_command(
      'AddMemoryDumpAssemblies',
      function(opts)
        M.StartAddMemoryDumpAssemblies(opts.args)
      end,
      { nargs = '?', complete='file' }
  )
  vim.api.nvim_create_user_command(
      'UniqCallStacks',
      function(opts)
        M.StartUniqCallStacks()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'SearchForType',
      function(opts)
        M.StartGetAllTypes(opts.args)
      end,
      { nargs = 1 }
  )
  vim.api.nvim_create_user_command(
      'OpenDecompilerLog',
      function(opts)
        M.OpenLog()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'StartDecompiler',
      function(opts)
        M.StartDecompiler()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'StartDecompilerFromMemoryDump',
      function(opts)
        M.StartFromMemoryDump(opts.args)
      end,
      { nargs = '?', complete='file'}
  )
  vim.api.nvim_create_user_command(
      'StartDecompilerFromProcess',
      function(opts)
        M.StartFromProcess(opts.args)
      end,
      { nargs = 1 }
  )
  vim.api.nvim_create_user_command(
      'StartDecompilerNoSolution',
      function(opts)
        M.StartNoSolution()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'GetAssemblies',
      function(opts)
        M.StartGetAssemblies()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'DecompileAssembly',
      function(opts)
        M.StartGetAssembliesForDecompile()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'DeleteDecompileLog',
      function(opts)
        M.ClearLog()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'ClearNugetDirectory',
      function(opts)
        M.ClearNugetDirectory()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'SearchNugetAndDecompile',
      function(opts)
        M.StartSearchNuget(opts.fargs[1], false)
      end,
      { nargs = '*' }
  )
  vim.api.nvim_create_user_command(
      'SearchNugetAndDecompileWithDependencies',
      function(opts)
        M.StartSearchNuget(opts.fargs[1], true)
      end,
      { nargs = '*' }
  )
  vim.api.nvim_create_user_command(
      'RefreshDecompiledBuffer',
      function(opts)
        M.StartRefreshBuffer()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
    'SearchNugetFromLocation',
    function(opts)
      M.StartSearchNugetFromLocation(opts.args)
    end,
    { nargs = '?' }
  )
  vim.api.nvim_create_user_command(
    'AddNugetSource',
    function(opts)
      M.AddNugetSource(opts.args)
    end,
    { nargs = 1 }
  )
  vim.api.nvim_create_user_command(
    'FindMethodByName',
    function(opts)
      M.StartFindMethodByName(opts.fargs[1], opts.fargs[2], opts.fargs[3])
    end,
    { nargs = '*' }
  )
  vim.api.nvim_create_user_command(
    'FindMethodByStackFrame',
    function(opts)
      M.StartFindMethodByStackFrame()
    end,
    {}
  )
  vim.api.nvim_create_user_command(
    'FindMethodByFilePath',
    function(opts)
      M.StartFindMethodByFilePath()
    end,
    {}
  )
  vim.api.nvim_create_user_command(
      'SearchMembers',
      function(opts)
        M.StartSearchMembers({ opts.fargs[1] }, opts.fargs[2])
      end,
      { nargs = '+' }
  )
  vim.api.nvim_create_user_command(
      'SearchMembersTelescope',
      function(opts)
        M.StartGetAssembliesForSearchMembers(opts.args)
      end,
      { nargs = 1 }
  )
end

return M
