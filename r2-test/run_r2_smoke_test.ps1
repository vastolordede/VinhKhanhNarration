$ErrorActionPreference = "Stop"

py -m pip install -r "$PSScriptRoot\requirements-r2-smoke-test.txt"
py "$PSScriptRoot\r2_smoke_test.py"

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}
