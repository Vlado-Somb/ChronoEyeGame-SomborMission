import {validateDocument,verifyLinks} from './core.mjs';
import {identityErrors,flowTaskErrors} from './identity.mjs';
export async function checkCandidate(p,next,previous,read) {
 const local=validateDocument(p,next);
 if(!local.ok)return local;
 const get=name=>name===p?Promise.resolve(next):read(name);
 const all={waypoints:await get('waypoints.json'),sources:await get('sources.json'),characters:await get('systems/characters.json'),missions:[],dialogues:[],flows:[]};
 for(let i=0;i<7;i++){
  all.missions.push(await get('missions/act-'+i+'.json'));
  all.dialogues.push(await get('dialogues/act-'+i+'.json'));
  all.flows.push(await get('production/act-'+i+'/scene-flow.json'));
 }
 const links=verifyLinks(all);
 const errors=[...identityErrors(p,previous,next),...links.errors,...flowTaskErrors(all.missions,all.flows,all.dialogues)];
 return {ok:!errors.length,errors,warnings:[...local.warnings,...links.warnings],counts:links.counts};
}
