local pickers = require('telescope.pickers')
local finders = require('telescope.finders')
local sorters = require('telescope.sorters')
local actions = require('telescope.actions')
local actions_state = require('telescope.actions.state')
local job = require('plenary.job')

local M = {}

M._updateProgress = function(action, level, message)
  vim.fn.setqflist({}, action, { lines = { level .. ':' .. message }}) 
  vim.cmd('normal G')
end

M._splitOnSpace = function(val)
  local parts = {}

  for i in string.gmatch(val, "%S+") do
    table.insert(parts, i)
  end
  return parts
end

M._cleanNugetResults = function(results)
  local cleanedResults = {}
  for i,v in ipairs(results) do
    if(not string.find(v, "Provider is applicable for")
      and not string.find(v, "The credential plugin model used by")
      and not string.find(v, "using msbuild version")) then
      table.insert(cleanedResults, v)
    end
  end
  return cleanedResults
end

M._displaySelector = function(items, selectionFn)
  local opts = {
    finder = finders.new_table {
      results = items
    },
    sorter = sorters.get_generic_fuzzy_sorter(),
    attach_mappings = function(prompt_bufnr, map)
      actions.select_default:replace(function()
        actions.close(prompt_bufnr)
        local selection = actions_state.get_selected_entry()
        local selectionParts = M._splitOnSpace(selection.value)
        selectionFn(selectionParts)
      end)
      return true
    end,
  }
  local picker = pickers.new(opts)
  local s = picker:find()
end

M.SearchNuget = function(searchString, searchAllVersions)
  vim.cmd("botright copen")
  M._updateProgress('r', 'INFO', 'Starting Nuget Search')
  if(require('csdecompile')._state.StartSent == false) then
    M._updateProgress('a', 'ERRO', 'Decompiler not started. Run StartDecompiler or StartDecompilerNoSolution')
    return
  end

  M._searchNuget(searchString, searchAllVersions)
end

M._installPackage = function(packageName, version)
  M._updateProgress('a', 'INFO', 'Starting to install package: ' .. packageName .. ' version: ' .. version)
  job:new({
    command = 'nuget',
    args = { 'install', packageName, '-version', version, '-source', M._nugetFeedUri, '-DependencyVersion', 'ignore', '-NoCache', '-OutputDirectory', M._packageDirectory},
    on_exit = vim.schedule_wrap(function(install, installReturnVal)
      M._updateProgress('a', 'INFO', 'install completed')
      for k,v in ipairs(install:result()) do
        M._updateProgress('a', 'INFO', v)
      end
      M._updateProgress('a', 'INFO', 'install nuget command completed with return code ' ..installReturnVal)

      if(installReturnVal == 0) then
        local installDir = M._packageDirectory .. packageName .. '.' .. version
        M._updateProgress('a', 'INFO', 'Adding dll directory to decompiler: ' ..installDir)
        require('csdecompile').StartAddExternalDirectory(installDir)
        M._updateProgress('a', 'INFO', 'Added dll directory to decompiler: ' .. installDir)
        vim.cmd('cclose')
        M._updateProgress('a', 'INFO', 'Successfully added ' ..installDir .. ' to decompiler ')
      else
        M._updateProgress('a', 'INFO', 'Skipping adding dlls to decompiler since nuget command failed with return code ' .. installReturnVal)
      end
    end)
  }):start()
end

M._searchNuget = function(searchString, searchAllVersions)
  job:new({
    command = 'nuget',
    args = { 'list', searchString, '-source', M._nugetFeedUri },
    on_exit = vim.schedule_wrap(function(j, return_val)
      local cleanedResults = M._cleanNugetResults(j:result())
      M._updateProgress('a', 'INFO', 'Done Searching')

      M._displaySelector(cleanedResults, function(selectionParts)
        if(searchAllVersions) then
          M._searchNugetForVersions(selectionParts[1])
        else
          M._installPackage(selectionParts[1], selectionParts[2])
        end
      end)
    end)
  }):start()
  M._updateProgress('a', 'INFO', 'Starting nuget search. searchString: ' .. searchString .. ' searchAllVersions: ' .. tostring(searchAllVersions))
end

M._searchNugetForVersions = function(packageName)
  job:new({
    command = 'nuget',
    args = { 'list', '"packageName"', '-source', M._nugetFeedUri, '-AllVersions'},
    on_exit = vim.schedule_wrap(function(j, return_val)
      local cleanedResults = M._cleanNugetResults(j:result())
      M._updateProgress('a', 'INFO', 'Done Searching')

      M._displaySelector(cleanedResults, function(selectionParts)
        M._installPackage(selectionParts[1], selectionParts[2])
      end)
    end)
  }):start()

  M._updateProgress('a', 'INFO', 'Starting search for ' .. packageName .. ' versions. feed: ' .. M._nugetFeedUri)
end

M.CleanPackageDirectory = function()
  local cmd = '!rd /s /q ' .. M._packageDirectory
  vim.cmd(cmd)
end

M.setup = function(opts)
  if opts == nil then
    opts = {}
  end

  M._packageDirectory = vim.F.if_nil(
  opts._packageDirectory,
  string.format("%s\\%s\\packages\\", vim.api.nvim_call_function("stdpath", { "cache" }), 'csdecompile.nuget')
  )

  M._nugetFeedUri = vim.F.if_nil(
  opts._nugetFeedUri,
  'https://api.nuget.org/v3/index.json'
  )

  vim.api.nvim_create_user_command(
  "SearchNuget",
  function(args)
    M.SearchNuget(args.args, false)
  end,
  { nargs = 1 })

  vim.api.nvim_create_user_command(
  "SearchNugetAllVersions",
  function(args)
    M.SearchNuget(args.args, true)
  end,
  { nargs = 1 })

  vim.api.nvim_create_user_command(
  "CleanPackageDirectory",
  function(args)
    M.CleanPackageDirectory()
  end,
  {})
end

return M
