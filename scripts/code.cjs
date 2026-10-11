// usage: node code.cjs file.cs [none]  -> ejecuta C# en Revit (cuerpo de Execute(document, parameters, uiapp))
const net=require('net'),fs=require('fs');
const code=fs.readFileSync(process.argv[2],'utf8');const params={code,parameters:[]};if(process.argv[3])params.transactionMode=process.argv[3];
const s=net.connect(8080,'127.0.0.1',()=>s.write(JSON.stringify({jsonrpc:'2.0',method:'send_code_to_revit',params,id:'1'})));
let b='';s.on('data',d=>{b+=d;try{const r=JSON.parse(b);s.end();const x=r.result||r.error;if(x.success){let o=x.result;try{o=JSON.parse(o)}catch{};console.log(typeof o==='string'?o:JSON.stringify(o,null,1))}else console.log('FALLO',x.errorMessage||x.message||JSON.stringify(x));process.exit(0)}catch{}});
s.on('error',e=>{console.log('ERR',e.message);process.exit(1)});setTimeout(()=>{console.log('TIMEOUT');process.exit(1)},180000);
