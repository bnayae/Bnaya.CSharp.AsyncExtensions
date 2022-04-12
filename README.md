# Bnaya.CSharp.AsyncExtensions [![NuGet](https://img.shields.io/nuget/v/Bnaya.CSharp.AsyncExtensions.svg)](https://www.nuget.org/packages/Bnaya.CSharp.AsyncExtensions/)  [![Prepare](https://github.com/weknow-network/Bnaya.CSharp.AsyncExtensions/actions/workflows/prepare-nuget.yml/badge.svg)](https://github.com/weknow-network/Bnaya.CSharp.AsyncExtensions/actions/workflows/prepare-nuget.yml) [![Deploy](https://github.com/weknow-network/Bnaya.CSharp.AsyncExtensions/actions/workflows/build-publish.yml/badge.svg)](https://github.com/weknow-network/Bnaya.CSharp.AsyncExtensions/actions/workflows/build-publish.yml)
Contribution of useful async pattern, API, Extensions, etc.

# NuGet
this library available on NuGet via
Install-Package Bnaya.CSharp.AsyncExtensions

## This library have the following godies:
* Exception Handlinfg
  * ThrowAll (produce AggregateException when waiting on Task.WhenAll)
  * Format (format async exception into friendlier callstack represantation)
* Timeout (will apply timeout semantic for any Task)
  * WithTimeout (will throw on timeout)
  * IsTimeoutAsync (will return indication without throwing, ideal for SLA checks [prctice: check and produce warning])
* Cancellation
  * CancelSafe (will run the CancellationTokenSource.Cancel within try catch and prevent unexpected side effect which can happen when cancellation token registration throw)
  * RegisterWeak (use week registration to cancellation token, letting the GC to collect the instance of the registration even when the token is still alive, can reduce potential for memory leaks)
* Frendly async locking facilities (which can replace the clasical lock statement).
  * Extensions
    * TryAcquireAsync
    * AcquireAsync
  * Instance-able
    * AsyncLock 


Looking for other extensions?  
Check the following
- [Json extenssions](https://github.com/weknow-network/Weknow-Json-Extensions)
- [Async extensions](https://github.com/weknow-network/Bnaya.CSharp.AsyncExtensions)
- [DI extensions](https://github.com/weknow-network/Weknow-DI-Extensions)
