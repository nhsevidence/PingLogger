# PingLogger
  
 > Log details of every request to Kibana to find the root cause of 'external service interactions'
 
<details>
<summary><strong>Table of contents</strong></summary>
<!-- START doctoc -->
- [PingLogger](#pinglogger)
  - [What is it?](#what-is-it)
    - [External service interactions](#external-service-interactions)
  - [Stack](#stack)
  - [Set up](#set-up)
  - [Usage](#usage)
<!-- END doctoc -->
</details>
  
## What is it?

PingLogger is a small web app that logs details of every request to Kibana. Its aim is to allow us to diagnose 'external service interaction' vulnerabilities that have been raised in pen tests, and identify where they are coming from.

The idea is to deploy it to an internal only URL (for example ping-logger.nice.org.uk or similar). This URL can then be used in a request header (e.g. *X-Forwarded-For* or *Host*) where there's an external service interaction vulnerability, which in turn should trigger a log of that request to Kibana.

> Note: there's no automated build or deployment for this project and it's not permanently running, as it's only needed on demand to diagnose these issues. So speak to Ops if you need it for testing.

### External service interactions

By passing an arbitrary domain name in a header (e.g. *X-Forwarded-For* or *Host*) in a request, it can be possible to induce an application to perform a server-side DNS lookup or HTTP request to the specified domain. This won't necessarily be a vulnerability in its own right, but could leak extra information (e.g. internal IP addresses etc) to the given URL which could then be used maliciously. This could be used for a Server-Side Request Forgery (SSRF) attack.

A further explanation, from [portswigger](https://portswigger.net/kb/issues/00300200_external-service-interaction-dns):

> External service interaction arises when it is possible to induce an application to interact with an arbitrary external service, such as a web or mail server. The ability to trigger arbitrary external service interactions does not constitute a vulnerability in its own right, and in some cases might even be the intended behavior of the application. However, in many cases, it can indicate a vulnerability with serious consequences.
  
## Stack

- Visual Studio 2017+
- ASP.NET Core 2.1
- NICE Logging
- Serilog
  
## Set up

1. Clone with Git and open *src/PingLogger.sln* in Visual Studio 2017+
2. Edit user secrets to put in logging configuration values
   1. You can find these from the *Logging* variable set in Octo or from another private project with NICE Logging configured.
3. Make sure the internal NICE NuGet feed is configured as a package source in Visual Studio
4. Build the solution, which restores packages
5. Run the application and you'll see details of each request appear in Kibana.

> Note: this is a public repository, which means we don't store any real config in appsettings.json. Instead, all config on dev machines is managed via secrets.json.

## Usage

Get it deployed to a URL (e.g. ping-logger.nice.org.uk), via Ops. Then use that URL in a request header (e.g. *X-Forwarded-For* or *Host*), for example:

```
GET /some-url HTTP/1.1
Host: www.nice.org.uk
User-Agent: Whatever/1.2.3
X-Forwarded-For: ping-logger.nice.org.uk
```

If the given URL is pinged, you'll see details of the request in Kibana