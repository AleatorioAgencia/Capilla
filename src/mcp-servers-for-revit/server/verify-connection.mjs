import { Client } from '@modelcontextprotocol/sdk/client/index.js';
import { StdioClientTransport } from '@modelcontextprotocol/sdk/client/stdio.js';
import { fileURLToPath } from 'node:url';

const client = new Client({ name: 'capilla-connection-check', version: '1.0.0' });
const transport = new StdioClientTransport({
  command: process.execPath,
  args: [fileURLToPath(new URL('./build/index.js', import.meta.url))],
  stderr: 'pipe',
});
try {
  await client.connect(transport);
  const { tools } = await client.listTools();
  console.log(JSON.stringify({ server: 'revit', toolCount: tools.length, tools: tools.map(t => t.name) }));
  const view = await client.callTool({ name: 'get_current_view_info', arguments: {} });
  console.log(JSON.stringify({ view }));
  const model = await client.callTool({ name: 'send_code_to_revit', arguments: {
    code: 'return Newtonsoft.Json.JsonConvert.SerializeObject(new { title = document.Title, path = document.PathName, revitVersion = uiapp.Application.VersionNumber, modified = document.IsModified });',
    parameters: [], transactionMode: 'none',
  } });
  console.log(JSON.stringify({ model }));
} finally {
  await client.close();
}
process.exit(0);
