# cs-vigil-reporter

#### Vigil Reporter for C#. Used in pair with [Vigil](https://github.com/valeriansaliou/vigil), the Microservices Status Page.


## Who uses it?

<table>
<tr>
<td align="center"><a href="https://r3-iot.com/"><img src="https://r3-iot.com/imgs/logo.svg" height="32" /></a></td>
</tr>
<tr>
<td align="center">R3 IoT Ltd.</td>
</tr>
</table>



# How to install
Install with nuget:

```sh
$ nuget install CSVigilReporter -OutputDirectory packages
```


# How to use
`VigilReporter` can be instantiated as such:

```c#
using VigilReporter;

var reporter = new VigilReporter("<url>", "<secretToken>", "<probeId>", "<nodeId>", "<replicaId>", <interval>);

reporter.StartBackgroundReporting();
```

# What is Vigil?
ℹ️ **Wondering what Vigil is?** Check out **[valeriansaliou/vigil](https://github.com/valeriansaliou/vigil)**.