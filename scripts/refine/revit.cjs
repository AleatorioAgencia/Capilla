const fs = require('node:fs');
const net = require('node:net');
function request(method, params = {}, timeout = 180000) {
  return new Promise((resolve, reject) => {
    let buffer = '', finished = false;
    const socket = net.createConnection({ host: '127.0.0.1', port: 8080 });
    const timer = setTimeout(() => finish(new Error('Revit timeout; inspect before retrying')), timeout);
    function finish(error, result) { if (finished) return; finished = true; clearTimeout(timer); socket.destroy(); error ? reject(error) : resolve(result); }
    socket.on('connect', () => socket.write(JSON.stringify({ jsonrpc: '2.0', method, params, id: String(Date.now()) })));
    socket.on('data', data => { buffer += data; let response; try { response = JSON.parse(buffer); } catch { return; } response.error ? finish(new Error(response.error.message)) : finish(null, response.result); });
    socket.on('error', error => finish(error));
  });
}
async function run(code, mode = 'none') {
  const params = { code, parameters: [], transactionMode: mode };
  if (Buffer.byteLength(JSON.stringify(params)) > 7800) throw new Error('Snippet exceeds bridge buffer');
  const r = await request('send_code_to_revit', params);
  if (!r.success) throw new Error(r.errorMessage || JSON.stringify(r));
  let value = r.result;
  for (let i = 0; i < 4 && typeof value === 'string'; i++) { try { value = JSON.parse(value); } catch { break; } }
  return value;
}
module.exports = { request, run };
if (require.main === module) run(fs.readFileSync(process.argv[2], 'utf8'), process.argv[3] || 'none').then(v => { console.log(JSON.stringify(v, null, 2)); }, e => { console.error(e); process.exitCode = 1; });
