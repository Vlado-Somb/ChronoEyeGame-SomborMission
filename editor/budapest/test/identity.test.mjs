import test from 'node:test';
import assert from 'node:assert/strict';
import { identityErrors } from '../lib/identity.mjs';
test('editorial text may change without changing stable IDs', () => {
  const original={id:'M',tasks:[{id:'T',title:'Old'}]};
  const edited={id:'M',tasks:[{id:'T',title:'New'}]};
  assert.equal(identityErrors('missions/act-0.json',original,edited).length,0);
  edited.tasks[0].id='T2';
  assert.equal(identityErrors('missions/act-0.json',original,edited).length,1);
});
