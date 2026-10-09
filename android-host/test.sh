#!/bin/sh
set -eu
cd "$(dirname "$0")"
build_dir=$(mktemp -d)
trap 'rm -rf "$build_dir"' EXIT
java -m jdk.compiler/com.sun.tools.javac.Main --release 11 -d "$build_dir" src/main/java/rs/chronoeye/host/SessionEngine.java src/test/java/rs/chronoeye/host/SessionEngineTest.java
java -cp "$build_dir" rs.chronoeye.host.SessionEngineTest
node --check web/host-map-adapter.js
node --test web/host-map-adapter.test.cjs
