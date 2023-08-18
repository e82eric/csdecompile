# csdecompile
C# Code navigation for Neovim that decompiles through references using Roslyn and IlSpy Telescope.

- GotoDefinition
- FindUsages
- FindImplementations
- ListTypeMembers
- SearchForType
- SearchForMembers
- GetAssemblies
- DecompileAssembly

Installation Steps
1. Add plugin ex. Plug 'e82eric/csdecompile'
2. Build .net navigation back end by running build.ps1

Starting the back end.
:StartDecompiler
- This will start the decompiler using the solution file found in the current directory.  If multiple solution files are found a dialog to choose the correct solution will be shown
:StartNoSolution
- This will start the decompiler with no solution and no assemblies.  Usually you would run AddExternalAssemblyDirectory after to load a set of dlls to navigate through SearchForType or SearchForMembers

Experimental Stuff:
:SearchNugetAndDecompile/SearchNugetAndDecompileWithDependencies
- using the provided search string this will display a list of packages found using configured nuget feed and load the containing dlls into the decompiler workspace
:ParseStackTrace/FindMethodByFilePath/FindMethodByStackFrame
- ParseStackTrace will format and syntax highlight a visually selected stack trace.  You can use FindMethodByFilePath and FindMethodByStackFrame to navigate to the location under the cursor (assuming that the project or dll is loaded into the workspace).

Configuration:

This is what I am currently using to configure the plugin
```
local function decompiler_status()
  return require('csdecompile').GetSolutionLoadingStatus()
end

local function decompiler_operation_status()
  return require('csdecompile').GetCurrentOperationMessage()
end

require('lualine').setup {
  options = {
    refresh = {
      statusline = 1000,
    }, theme = 'gruvbox_dark'
  },
  sections = {
    lualine_a = {'mode'},
    lualine_b = {'branch', 'diff', 'diagnostics'},
    lualine_c = {'filename'},
    lualine_x = { { decompiler_status, color = require('csdecompile').GetSolutionLoadingColor }, { decompiler_operation_status, color = require('csdecompile').GetOperationStatusColor }, 'encoding', 'fileformat', 'filetype'},
    lualine_y = {'progress'},
    lualine_z = {'location'}
  },
  inactive_sections = {
    lualine_a = {},
    lualine_b = {},
    lualine_c = {'filename'},
    lualine_x = {'location'},
    lualine_y = {},
    lualine_z = {}
  },
  tabline = {},
  winbar = {},
  inactive_winbar = {},
  extensions = {}
}

require('csdecompile').Setup({ logLevel = 'debug' })
require('csdecompile.stacktrace').setup()
```
These are the mappings that I am currently using in my cs.lua
```
vim.keymap.set("n", "<leader>gd", function()
  require('csdecompile').StartDecompileGotoDefinition()
end,
{ buffer=true })

vim.keymap.set("n", "<leader>fu", function()
  require('csdecompile').StartFindUsages()
end,
{ buffer=true })

vim.keymap.set("n", "<leader>fi", function()
  require('csdecompile').StartFindImplementations()
end,
{ buffer=true })

vim.keymap.set("n", "<leader>tm", function()
  require('csdecompile').StartGetTypeMembers()
end,
{ buffer=true })

vim.keymap.set("n", "<leader><leader>", function()
  require('csdecompile').StartGetSymbolName()
end,
{ buffer=true }}
```
