require'plenary.job'
local pickers = require "telescope.pickers"
local finders = require "telescope.finders"
local conf = require("telescope.config").values
local actions = require "telescope.actions"
local action_state = require "telescope.actions.state"
local previewers = require "telescope.previewers"
local entry_display = require("telescope.pickers.entry_display")
local strings = require "plenary.strings"
local Job = require('plenary.job')
-- local log = require('csdecompile.log')

local M = {}

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
}

M.OpenLog = function()
  local outfile = string.format("%s/%s.log", vim.api.nvim_call_function("stdpath", { "cache" }), M.log.plugin)
	local vimScriptCommand = 'e ' .. outfile
	vim.cmd(vimScriptCommand)
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
	local timer = vim.loop.new_timer()
	timer:start(1000, 0, vim.schedule_wrap(function()
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
	end))
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
		-- statusString = M._state.SolutionName .. ' loading...' .. numberOfProjectsString
		statusString = M._state.SolutionName .. ' loading... ' .. numberOfProjectsString
	elseif M._state.SolutionLoadingState == "done" then
		statusString = M._state.SolutionName .. ' ' .. numberOfProjectsString
	end
	local result = 'O#: ' .. statusString
	return result
end

local on_output = function(err, data)
	local timer = vim.loop.new_timer()
	timer:start(100, 0, vim.schedule_wrap(function()
		M.log.debug(data)
	end))
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
				commandCallback(json, data)
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

M._sendStdIoRequest = function(request, callback, callbackData)
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
   if string.match(curChar, "%W") then
     result = currentCol + 2
     break
   end
   currentCol = currentCol - 1
 end

 return result
end

M._decompileRequest = function(url, callback, callbackData)
	local cursorPos = vim.api.nvim_win_get_cursor(0)
	local line = cursorPos[1]
	local column = cursorPos[2] + 1
  column = M._getStartOfCurrentWord()
	local assemblyFilePath = vim.b.AssemblyFilePath
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
			AssemblyName = vim.b.AssemblyName,
			Type = locationType,
			ContainingTypeFullName = vim.b.ContainingTypeFullName,
			Column = column,
			Line = line,
		},
	}
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

M._openAssembliesTelescope = function(data, resultHandler)
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
	local timer = vim.loop.new_timer()
	timer:start(1000, 0, vim.schedule_wrap(function()
	pickers.new(opts, {
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
			actions.select_default:replace(function()
				local selection = action_state.get_selected_entry()
				actions.close(prompt_bufnr)
				resultHandler(selection.value.FilePath, selection.value.FullName)
			end)
			return true
		end,
		sorter = conf.generic_sorter(opts),
	}):find()
	end))
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

M.HandleGetAllTypes = function(response)
	M._openTelescope(response.Body.Implementations, M._createSearchTypesDisplayer, 'Search Types')
end

M.StartDecompileGotoDefinition = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest('/gotodefinition', M.HandleDecompileGotoDefinitionResponse)
end

M.StartFindUsages = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest("/findusages", M.HandleUsages)
end

M.HandleUsages = function(response)
	M._openTelescope(response.Body.Implementations, M._createUsagesDisplayer, 'Find Usages')
end

M.StartGetSymbolName = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest("/symbolinfo", M.HandleGetSymbolName)
end

M.HandleGetSymbolName = function(response)
	local timer = vim.loop.new_timer()
	timer:start(100, 0, vim.schedule_wrap(function()
		M._navigationFloatingWin(response.Body)
	end))
end

M.StartGetDecompiledSource = function(
	assemblyFilePath,
	containingTypeFullName,
	line,
	column,
	callbackData)

	local request = {
		Command = "/decompiledsource",
		Arguments = {
			AssemblyFilePath = assemblyFilePath,
			ContainingTypeFullName = containingTypeFullName,
			Line = line,
			Column = column,
		},
		Seq = M._state.NextSequence,
	}

	M._sendStdIoRequest(request, M.HandleDecompiledSource, callbackData)
end

M.StartGetTypeMembers = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest('/gettypemembers', M.HandleGetTypeMembers)
end

M.HandleGetTypeMembers = function(response)
	M._openTelescope(response.Body.Implementations, M._createUsagesDisplayer, 'Type Members')
end

M.StartFindImplementations = function()
  if M._checkNotRunning() then
    return
  end
	M._decompileRequest('/findimplementations', M.HandleFindImplementations)
end

M.HandleFindImplementations = function(response)
	if #response.Body.Implementations == 0 then
		print 'No implementations found'
	elseif #response.Body.Implementations == 1 then
		local timer = vim.loop.new_timer()
		timer:start(1000, 0, vim.schedule_wrap(function()
			M._openSourceFileOrDecompile(response.Body.Implementations[1])
		end))
	else
		M._openTelescope(response.Body.Implementations, M._createUsagesDisplayer, 'Find Implementations')
	end
end

M._openSourceFileOrDecompile = function(value)
	if value.Type == 1 then
		local bufnr = vim.uri_to_bufnr(value.FileName)
		vim.api.nvim_win_set_buf(0, bufnr)
		vim.api.nvim_win_set_cursor(0, { value.Line, value.Column })
	else
		M.StartGetDecompiledSource(
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
	local fileName = location.FileName

	if location.Type == 1 then
		M._openSourceFile(location)
	else
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
	local timer = vim.loop.new_timer()
	timer:start(100, 0, vim.schedule_wrap(function()
		local bufnr = vim.uri_to_bufnr(location.FileName)
		vim.api.nvim_win_set_buf(0, bufnr)
		vim.api.nvim_buf_set_option(bufnr, "buflisted", true)
		M._setCursorFromLocation(0, location)
	end))
end

M._setBufferTextFromDecompiledSource = function(location, sourceText, bufnr, winid)
	local lines = {}
	local decompileFileName = ''
	local fileName = location.FileName
	local column = location.Column - 1

	local timer = vim.loop.new_timer()
	timer:start(100, 0, vim.schedule_wrap(function()
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
		vim.b.ContainingTypeFullName = location.ContainingTypeFullName
	end))
end

M._blankIfNil = function(val)
	local result = ''
  if val ~= nil then
    if type(val) == "string" then
      return val
    else
      return '[object]'
    end
  end
	return ''
end

M._createFindImplementationsDisplayer = function(widths)
	local resultFunc = function(entry)
		local make_display = nil
		if entry.Type == 0 then
			local displayer = entry_display.create {
				separator = "  ",
				items = {
					{ width = widths.ContainingTypeFullName },
					{ width = widths.NamespaceName },
					{ width = widths.AssemblyName + widths.AssemblyVersion + 1 },
					{ width = widths.DotNetVersion + 6 },
					{ remaining = true },
				},
			}

			make_display = function(entry)
				return displayer {
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
					{ width = widths.ContainingTypeFullName },
					{ width = widths.NamespaceName },
					{ remaining = true },
				},
			}

			make_display = function(entry)
				return displayer {
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

M._createSearchTypesDisplayer = function(widths)
	local resultFunc = function(entry)
		local displayer = entry_display.create {
			separator = "  ",
			items = {
				{ width = widths.SourceText },
				{ width = widths.AssemblyFilePath },
				{ remaining = true },
			},
		}

		local make_display = function(entry)
			return displayer {
				{ M._blankIfNil(entry.value.SourceText), "TelescopeResultsClass" },
				{ M._blankIfNil(entry.value.AssemblyName), "TelescopeResultsClass" },
				{ M._blankIfNil(entry.value.AssemblyFilePath), "TelescopeResultsIdentifier" }
			}
		end

		return {
			value = entry,
			display = make_display,
			ordinal = entry.SourceText
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
					{ width = widths.ContainingTypeShortName },
					{ remaining = true },
				}
			}

			ordinal = M._blankIfNil(entry.FileName) .. M._blankIfNil(entry.SourceText)
			make_display = function(entry)
				return displayer {
					{ string.format("%s", entry.value.ContainingTypeShortName), "TelescopeResultsClass" },
					{ string.format("%s", entry.value.SourceText), "TelescopeResultsIdentifier" },
				}
			end
		else
			local displayer = entry_display.create {
				separator = "  ",
				items = {
					{ width = widths.ContainingTypeShortName },
					{ remaining = true },
					-- { remaining = true },
				},
			}

			ordinal = M._blankIfNil(entry.ContainingTypeFullName) .. M._blankIfNil(entry.SourceText)
			make_display = function(entry)
				return displayer {
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

M._openTelescope = function(data, displayFunc, promptTitle)
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
	local timer = vim.loop.new_timer()
	timer:start(1000, 0, vim.schedule_wrap(function()
	pickers.new(opts, {
		layout_strategy='vertical',
		layout_config = {
			width = 0.95,
			height = 0.95,
		},
		prompt_title = promptTitle,
		finder = finders.new_table {
			results = data,
			entry_maker = entryMaker,
		},
		preview = opts.previewer,
		attach_mappings = function(prompt_bufnr, map)
			actions.select_default:replace(function()
				local selection = action_state.get_selected_entry()
				actions.close(prompt_bufnr)
				M._openSourceFileOrDecompile(selection.value)
			end)
			return true
		end,
		previewer = previewers.new_buffer_previewer {
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
						entry.value.AssemblyFilePath,
						entry.value.ContainingTypeFullName,
						entry.value.Line,
						entry.value.Column,
						{ Entry = entry.value, BufferNumber = bufnr, WindowId = winid, })

					vim.api.nvim_buf_set_option(self.state.bufnr, "syntax", "cs")
					vim.api.nvim_buf_set_lines(self.state.bufnr, 0, -1, false, { 'Decompiling ' .. entry.value.SourceText .. '...'})
				end
			end
		},
		sorter = conf.generic_sorter(opts),
	}):find()
	end))
end

M.Setup = function(config)
  if not config then
    config = {}
  end

  local logLevel = "warn"
  if config.logLevel then
    logLevel = config.logLevel
  end
  M.log = require("plenary.log").new({
    plugin = "csdecompile",
    level = logLevel
  })

  vim.api.nvim_create_user_command(
      'AddExternalAssemblyDirectory',
      function(opts)
        M.StartAddExternalDirectory(opts.args[0])
      end,
      { nargs = '?', complete='file' }
  )
  vim.api.nvim_create_user_command(
      'SearchForType',
      function(opts)
        M.StartGetAllTypes(opts.args[0])
      end,
      { nargs = '?', complete='file' }
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
      'StartDecompilerNoSolution',
      function(opts)
        M.StartNoSolution()
      end,
      {}
  )
  vim.api.nvim_create_user_command(
      'StartGetAssemblies',
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
end

return M
