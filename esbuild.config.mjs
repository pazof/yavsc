// Yavsc front-end bundle config (esbuild).
//
// Produces a single minified bundle at
// src/Yavsc.Org/wwwroot/js/site.bundle.min.js
// that contains both our application code and the third-party libs we
// keep under npm. The bundle is committed to the repo so the .NET
// runtime does not need a node toolchain at publish time.
//
// Usage:
//   npm install
//   npm run build:js          # one-shot
//   npm run build:js:watch    # rebuild on change
//   npm run clean:js          # remove the generated bundle

import { build, context } from 'esbuild';
import { existsSync, mkdirSync, statSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const isWatch = process.argv.includes('--watch');

const outFile = path.resolve(
  __dirname,
  'src/Yavsc.Org/wwwroot/js/site.bundle.min.js'
);

const outDir = path.dirname(outFile);
if (!existsSync(outDir)) {
  mkdirSync(outDir, { recursive: true });
}

// --- Our code (entry points) --------------------------------------------
// `site.js` is the bootstrap loaded by _Layout.cshtml. We list every
// .js we own as an entry point so they all end up in the same bundle.
const ourEntries = [
  'src/Yavsc.Org/wwwroot/js/site.js',
  'src/Yavsc.Org/wwwroot/js/chat.js',
  'src/Yavsc.Org/wwwroot/js/comment.js',
  'src/Yavsc.Org/wwwroot/js/signout-redirect.js',
  'src/Yavsc.Org/wwwroot/js/input-lib.js',
  'src/Yavsc.Org/wwwroot/js/google-geoloc.js',
  'src/Yavsc.Org/wwwroot/js/audiovideoinput.js',
  'src/Yavsc.Org/wwwroot/js/yavsc-remote-fs.js',
];

// --- Third-party code we want bundled ----------------------------------
// Listed by npm package name; resolved via node_modules.
// jQuery, jQuery UI, jQuery plugins, Bootstrap JS bundle, SignalR
// (browser), Moment, Quill, Showdown, To-markdown, Dropzone.
const vendorEntries = [
  'jquery/dist/jquery.min.js',
  'jquery-ui/dist/jquery-ui.min.js',
  'jquery-validation/dist/jquery.validate.min.js',
  'jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js',
  'jquery-timepicker/jquery.timepicker.min.js',
  'jquery-datepair/dist/jquery.datepair.min.js',
  'bootstrap/dist/js/bootstrap.bundle.min.js',
  '@microsoft/signalr/dist/browser/signalr.min.js',
  'moment/min/moment.min.js',
  'showdown/dist/showdown.min.js',
  'to-markdown/dist/to-markdown.min.js',
  'dropzone/dist/min/dropzone.min.js',
  'quill/dist/quill.min.js',
  // eonasdan is registered as a CommonJS package; esbuild handles it.
  'eonasdan-bootstrap-datetimepicker/build/js/bootstrap-datetimepicker.min.js',
];

// Our entry points may live alongside the output, so we exclude the
// outDir from the resolver to avoid bundling the previous run.
const entryPoints = [...ourEntries, ...vendorEntries];

const buildOptions = {
  entryPoints,
  outFile,
  bundle: true,
  minify: true,
  sourcemap: false,
  target: ['es2018'],
  format: 'iife',
  legalComments: 'none',
  logLevel: 'info',
  // jQuery plugins attach themselves to jQuery; we must not shim it.
  alias: {},
  nodePaths: ['node_modules'],
  loader: {
    '.js': 'js',
  },
};

async function run() {
  if (isWatch) {
    const ctx = await context(buildOptions);
    await ctx.watch();
    console.log(`[esbuild] watching → ${path.relative(__dirname, outFile)}`);
  } else {
    const result = await build(buildOptions);
    if (result.errors.length) {
      process.exitCode = 1;
    } else {
      const size = statSync(outFile).size;
      console.log(
        `[esbuild] wrote ${path.relative(__dirname, outFile)} (${size} bytes)`
      );
    }
  }
}

run().catch((err) => {
  console.error(err);
  process.exit(1);
});
