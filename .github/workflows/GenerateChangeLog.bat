@echo off
For /f "tokens=1* delims=:" %%i in ('Type Changes.md^|Findstr /n ".*"') do (
    if "%%j"=="" (
        goto :end
    )
    if NOT "%%i"=="1" (
        echo %%j>>changes.txt
    )
)
:end