
@echo off
"C:\Program Files\Unity\Hub\Editor\2021.3.23f1c1\Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\Emscripten\emscripten\emcc.bat" %* < nul
exit %ERRORLEVEL%
