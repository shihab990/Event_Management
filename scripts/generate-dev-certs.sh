#!/usr/bin/env bash

set -euo pipefail

show_usage() {
  cat <<'EOF'
Usage: scripts/generate-dev-certs.sh

Creates localhost TLS assets under certs/ for docker-compose:
  - localhost.key / localhost.crt for Nginx
  - localhost.pfx for ASP.NET Core
  - local-dev-ca.pem copy for trusting in browsers

EOF
}

CN="localhost"
FORCE=false

while [[ $# -gt 0 ]]; do
  case "$1" in
    --cn)
      shift
      [[ $# -gt 0 ]] || { echo "Missing value for --cn" >&2; exit 1; }
      CN="$1"
      ;;
    --force)
      FORCE=true
      ;;
    -h|--help)
      show_usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      show_usage >&2
      exit 1
      ;;
  esac
  shift
done

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CERT_DIR="$ROOT_DIR/certs"
mkdir -p "$CERT_DIR"

PASSWORD="${ASPNETCORE_HTTPS_CERT_PASSWORD:-aspnetcoredev}"
VALID_DAYS="${CERT_VALID_DAYS:-825}"

CRT_PATH="$CERT_DIR/localhost.crt"
KEY_PATH="$CERT_DIR/localhost.key"
PFX_PATH="$CERT_DIR/localhost.pfx"
CA_PATH="$CERT_DIR/local-dev-ca.pem"

maybe_remove_path() {
  local path="$1"
  if [[ -d "$path" ]]; then
    if [[ "$FORCE" == true ]]; then
      rm -rf "$path"
    else
      echo "Directory exists where file is expected: $path" >&2
      echo "Remove it or rerun with --force to have it deleted automatically." >&2
      exit 1
    fi
  elif [[ -f "$path" ]]; then
    if [[ "$FORCE" == true ]]; then
      rm -f "$path"
    else
      echo "File already exists: $path (use --force to overwrite)" >&2
      exit 1
    fi
  fi
}

maybe_remove_path "$CRT_PATH"
maybe_remove_path "$KEY_PATH"
maybe_remove_path "$PFX_PATH"
maybe_remove_path "$CA_PATH"

echo "Generating new TLS assets for CN=${CN}"

openssl req -x509 -nodes -days "$VALID_DAYS" \
  -newkey rsa:2048 \
  -subj "/C=US/ST=Development/L=Local/O=EventManagement/OU=Dev/CN=${CN}" \
  -addext "subjectAltName=DNS:${CN}" \
  -keyout "$KEY_PATH" \
  -out "$CRT_PATH" >/dev/null 2>&1

cp "$CRT_PATH" "$CA_PATH"

openssl pkcs12 -export \
  -out "$PFX_PATH" \
  -inkey "$KEY_PATH" \
  -in "$CRT_PATH" \
  -passout "pass:${PASSWORD}" >/dev/null 2>&1

cat <<EOF

Created certificates in $CERT_DIR:
  - $(basename "$KEY_PATH")
  - $(basename "$CRT_PATH")
  - $(basename "$PFX_PATH")
  - $(basename "$CA_PATH")

Trust $CA_PATH once so browsers accept https://localhost certs.
Remember to keep these files out of source control (certs/ is gitignored).
EOF
