// usage: node rv.cjs <method> [params.json]
const net=require('net'),fs=require('fs');
const method=process.argv[2]; const params=process.argv[3]?JSON.parse(fs.readFileSync(process.argv[3],'utf8')):{};
const s=net.connect(8080,'127.0.0.1',()=>s.write(JSON.stringify({jsonrpc:'2.0',method,params,id:String(Date.now())})));
let b='';s.on('data',d=>{b+=d;try{const r=JSON.parse(b);console.log(process.env.RAW?JSON.stringify(r):JSON.stringify(r,null,1).slice(0,6000));s.end();process.exit(0)}catch{}});
s.on('error',e=>{console.log('ERR',e.message);process.exit(1)});setTimeout(()=>{console.log('TIMEOUT',b.slice(0,800));process.exit(1)},60000);
