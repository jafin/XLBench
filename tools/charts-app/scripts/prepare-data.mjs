// Reads docs/results-data.js (`window.XLBENCH_DATA = { ... };`, written by the benchmark publisher),
// checks its shape, and writes src/generated/results.json, which the app imports and bakes into the page.
// A malformed payload fails the build here, with a message that names the problem, instead of shipping a
// blank page.
import { mkdirSync, readFileSync, writeFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

const here = dirname(fileURLToPath(import.meta.url))
const src = process.env.XLBENCH_DATA ?? join(here, '..', '..', '..', 'docs', 'results-data.js')
const out = join(here, '..', 'src', 'generated', 'results.json')

const fail = (msg) => { console.error(`[charts] ${msg}`); process.exit(1) }

let js
try { js = readFileSync(src, 'utf8') } catch { fail(`cannot read ${src}`) }
const start = js.indexOf('{'), end = js.lastIndexOf('}')
if (start < 0 || end <= start) fail(`no JSON object found in ${src}`)

let payload
try { payload = JSON.parse(js.slice(start, end + 1)) } catch (e) { fail(`${src} is not valid JSON: ${e.message}`) }

if (typeof payload.updated !== 'string') fail('payload.updated must be a string')
if (typeof payload.versions !== 'object' || !payload.versions) fail('payload.versions must be an object')
if (!Array.isArray(payload.scenarios)) fail('payload.scenarios must be an array')
for (const s of payload.scenarios) {
  if (typeof s.key !== 'string' || typeof s.label !== 'string') fail('each scenario needs a string key and label')
  const n = s.libraries?.length
  if (!Array.isArray(s.libraries)) fail(`scenario ${s.key}: libraries must be an array`)
  for (const f of ['timeMs', 'allocMb']) {
    if (!Array.isArray(s[f]) || s[f].length !== n) fail(`scenario ${s.key}: ${f} must have one entry per library (${n})`)
  }
  for (const f of ['errorMs', 'stdDevMs']) {
    if (s[f] !== undefined && (!Array.isArray(s[f]) || s[f].length !== n)) fail(`scenario ${s.key}: ${f} must have one entry per library (${n})`)
  }
  for (const lib of s.libraries) if (!(lib in payload.versions)) fail(`scenario ${s.key}: library "${lib}" is not in payload.versions`)
}
if (payload.scenarios.length === 0) console.warn('[charts] warning: payload has no scenarios; the page will show its empty state.')

mkdirSync(dirname(out), { recursive: true })
writeFileSync(out, JSON.stringify(payload))
console.log(`[charts] baked ${payload.scenarios.length} scenarios, ${Object.keys(payload.versions).length} libraries from ${src}`)
