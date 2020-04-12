@ECHO off

:: Get highest version number from folders names.
FOR /f "tokens=1, 2 delims=." %%i IN ('dir /ad /b /on') DO (
	SET /a a=%%i
	
	IF NOT "%%j"=="" (
		SET /a b=%%j
	)
)

:: Set the next version number.
SET /a b=%b%+1
SET c=%a%.%b%

:: Create next version folder if does not exists.
IF NOT EXIST "%CD%\%c%" (
	MKDIR %c%
)