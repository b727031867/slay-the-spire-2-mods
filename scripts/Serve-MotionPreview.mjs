// Serve one generated review artifact on loopback only; no directory or file browsing.
import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
const preview=path.resolve(import.meta.dirname,'../.local/motion039/preview.html');
http.createServer((request,response)=>{
    if(request.url==='/favicon.ico'){response.writeHead(204);response.end();return;}
    if(request.url!=='/'&&request.url!=='/preview.html'){response.writeHead(404);response.end();return;}
    response.writeHead(200,{'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store'});
    response.end(fs.readFileSync(preview));
}).listen(38939,'127.0.0.1',()=>console.log('Motion review: http://127.0.0.1:38939/preview.html'));
