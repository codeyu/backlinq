@echo off

REM BackLINQ
REM Copyright (c) 2008 Atif Aziz. All rights reserved.
REM
REM  Author(s):
REM
REM      Atif Aziz, http://www.raboof.com
REM
REM This library is free software; you can redistribute it and/or modify it 
REM under the terms of the New BSD License, a copy of which should have 
REM been delivered along with this distribution.
REM
REM THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
REM "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
REM LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
REM PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
REM OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
REM SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
REM LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
REM DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
REM THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
REM (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
REM OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
REM
REM -------------------------------------------------------------------------
REM

for %%i in (Debug Release) do (
    "%SystemRoot%\Microsoft.NET\Framework\v3.5\msbuild" /p:Configuration=%%i %~dp0src\BackLINQ.sln
)


