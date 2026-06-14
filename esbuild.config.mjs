// Yavsc front-end bundle config (esbuild).
//
// Produces several thematic minified bundles under
// src/Yavsc.Org/wwwroot/js/*.bundle.min.js
// plus static copies of assets that cannot be bundled (legacy
// SignalR 2.x client, moment-with-locales with its 137 dynamic
// requires).
//
// Each bundle has a single entry file under build/<name>.entry.js
// (gitignored, not committed) that re-exports the relevant vendor and
// our code. esbuild bundles from there into one IIFE per bundle.
//
// Usage:
//   npm install
//   npm run build:js          # one-shot
//   npm run build:js:watch    # rebuild on change
//   npm run clean:js          # remove generated bundles + static assets

import { build, context } from 'esbuild';
import { copyFileSync, existsSync, mkdirSync, rmSync, statSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

const isWatch = process.argv.includes('--watch');
const isClean = process.argv.includes('--clean');

const jsOutDir = path.resolve(__dirname, 'src/Yavsc.Org/wwwroot/js');
const buildDir = path.resolve(__dirname, 'build');

// One bundle per thematic area. Each one points at a single entry file
// under build/ that re-exports the relevant vendor + our code.
const bundleNames = [
  'core',       // jQuery + jQuery UI + Bootstrap 4 + validation + our core code
  'chat',       // core subset + chat + comment (SignalR is loaded separately as a static asset)
  'dropzone',   // jQuery + dropzone + yavsc-remote-fs
  'datetime',   // jQuery + jQuery UI + eonasdan datetimepicker + jquery-timepicker
                 // (moment-with-locales is a static asset, see copyStaticAssets)
  'timepicker', // jQuery + jQuery UI + jquery-timepicker
                 // (datepair is not on npm; kept as a static asset for now)
  'quill',      // Quill rich text editor (no jQuery dependency)
];

// Static assets that cannot be bundled (dynamic require, legacy
// server protocol, package not on npm). CSS files are also copied
// as static assets (bundling CSS is out of scope of this commit).
const staticAssets = [
  {
    src: 'node_modules/@aspnet/signalr/dist/browser/signalr.min.js',
    dst: 'signalr.min.js',
  },
  {
    src: 'node_modules/moment/min/moment-with-locales.min.js',
    dst: 'moment-with-locales.min.js',
  },
  {
    src: 'node_modules/bootstrap/dist/css/bootstrap.min.css',
    dst: '../css/bootstrap.min.css',
  },
  {
    src: 'node_modules/jquery-ui/dist/themes/base/jquery-ui.min.css',
    dst: '../css/jquery-ui.min.css',
  },
  {
    src: 'node_modules/dropzone/dist/min/dropzone.min.css',
    dst: '../css/dropzone.min.css',
  },
  {
    src: 'node_modules/dropzone/dist/min/basic.min.css',
    dst: '../css/dropzone-basic.min.css',
  },
  {
    src: 'node_modules/eonasdan-bootstrap-datetimepicker/build/css/bootstrap-datetimepicker.min.css',
    dst: '../css/bootstrap-datetimepicker.min.css',
  },
  {
    src: 'node_modules/jquery-timepicker/jquery.timepicker.css',
    dst: '../css/jquery.timepicker.css',
  },
];

function entryFor(bundleName) {
  return path.resolve(buildDir, `${bundleName}.entry.js`);
}

function outFor(bundleName) {
  return path.join(jsOutDir, `${bundleName}.bundle.min.js`);
}

function buildOptionsFor(bundleName) {
  return {
    entryPoints: [entryFor(bundleName)],
    outfile: outFor(bundleName),
    bundle: true,
    minify: true,
    sourcemap: false,
    target: ['es2018'],
    format: 'iife',
    legalComments: 'none',
    logLevel: 'info',
    nodePaths: ['node_modules'],
  };
}

function ensureDir(dir) {
  if (!existsSync(dir)) {
    mkdirSync(dir, { recursive: true });
  }
}

function copyStaticAssets() {
  for (const a of staticAssets) {
    const src = path.resolve(__dirname, a.src);
    const dst = path.join(jsOutDir, a.dst);
    if (!existsSync(src)) {
      console.warn(`[static] ${a.src} not installed; skipping`);
      continue;
    }
    copyFileSync(src, dst);
    console.log(`[static] ${a.dst} ← ${path.relative(__dirname, src)}`);
  }
}

async function run() {
  if (isClean) {
    for (const name of bundleNames) {
      const f = outFor(name);
      if (existsSync(f)) {
        rmSync(f);
        console.log(`[clean] removed ${path.relative(__dirname, f)}`);
      }
    }
    for (const a of staticAssets) {
      const f = path.join(jsOutDir, a.dst);
      if (existsSync(f)) {
        rmSync(f);
        console.log(`[clean] removed ${path.relative(__dirname, f)}`);
      }
    }
    return;
  }

  ensureDir(jsOutDir);

  if (isWatch) {
    for (const name of bundleNames) {
      const ctx = await context(buildOptionsFor(name));
      await ctx.watch();
      console.log(`[esbuild] watching ${name} → ${path.basename(outFor(name))}`);
    }
    copyStaticAssets();
    console.log('[esbuild] watch mode active');
  } else {
    for (const name of bundleNames) {
      const opts = buildOptionsFor(name);
      const result = await build(opts);
      if (result.errors.length) {
        console.error(`[esbuild] ${name} failed`);
        process.exitCode = 1;
        continue;
      }
      const size = statSync(opts.outfile).size;
      console.log(
        `[esbuild] ${name} → ${path.relative(__dirname, opts.outfile)} (${size} bytes)`
      );
    }
    copyStaticAssets();
  }
}

run().catch((err) => {
  console.error(err);
  process.exit(1);
});
