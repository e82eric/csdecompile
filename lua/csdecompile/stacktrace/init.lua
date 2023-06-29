local M = {}

M._tbl_length = function(T)
  local count = 0
  for _ in pairs(T) do count = count + 1 end
  return count
end

M._get_visual_selection = function()
  -- this will exit visual mode
  -- use 'gv' to reselect the text
  local _, csrow, cscol, cerow, cecol
  local mode = vim.fn.mode()
  if mode == 'v' or mode == 'V' or mode == '' then
    -- if we are in visual mode use the live position
    _, csrow, cscol, _ = unpack(vim.fn.getpos("."))
    _, cerow, cecol, _ = unpack(vim.fn.getpos("v"))
    if mode == 'V' then
      -- visual line doesn't provide columns
      cscol, cecol = 0, 999
    end
    -- exit visual mode
    vim.api.nvim_feedkeys(
    vim.api.nvim_replace_termcodes("<Esc>",
    true, false, true), 'n', true)
  else
    -- otherwise, use the last known visual position
    _, csrow, cscol, _ = unpack(vim.fn.getpos("'<"))
    _, cerow, cecol, _ = unpack(vim.fn.getpos("'>"))
  end
  -- swap vars if needed
  if cerow < csrow then csrow, cerow = cerow, csrow end
  if cecol < cscol then cscol, cecol = cecol, cscol end
  local lines = vim.fn.getline(csrow, cerow)
  -- local n = cerow-csrow+1
  local n = M._tbl_length(lines)
  if n <= 0 then return '' end
  lines[n] = string.sub(lines[n], 1, cecol)
  lines[1] = string.sub(lines[1], cscol)
  return lines
  -- return table.concat(lines, "\n")
end

M._parse_stack_frames = function(sourceLines)
  local result = {}
  for i, str in ipairs(sourceLines) do
    b = 1
    prevc = 1
    c = 1
    noMatch = true
    while true do
      local x,y = string.find(str, 'at [^ ]+%.[^ ]+%(-', b)
      if x == nil then
        local subStr = string.sub(str, prevc)
        table.insert(result, subStr)
        break
      end
      c = x
      local subStr = string.sub(str, prevc, c - 1)
      if c -1 > prevc then
        table.insert(result, subStr)
      end
      noMatch = false
      prevc = x
      b = y + 1
    end
  end
  return result
end

M._set_buffer = function(sourceLines)
  local decompileFileName = 'test.stacktrace'
  local newLines = M._parse_stack_frames(sourceLines)

  vim.cmd('belowright split')
  winid = 0
  bufnr = vim.uri_to_bufnr("c:\\TEMP\\" .. decompileFileName)
  vim.api.nvim_buf_set_lines(bufnr, 0, -1, false, newLines)
  vim.api.nvim_win_set_buf(winid, bufnr)
  vim.cmd('syntax clear')
  vim.cmd('hi Statement cterm=bold ctermfg=Green')
  vim.cmd([[syntax match stackFrameMatch /\vat \zs[^ ]+\..*\(.*\)\ze/]])
  vim.cmd('hi def link stackFrameMatch Special')
  vim.cmd([[syntax match filePathMatch /\vin \zs.*:line\ze/]])
  vim.cmd('hi def link filePathMatch Constant')
end

M.run = function()
  local lines = M._get_visual_selection()
  M._set_buffer(lines)
end

M.setup = function()
  vim.api.nvim_create_user_command(
  'ParseStackTrace',
  function(opts)
    M.run()
  end,
  {range = true}
  )
end

return M
