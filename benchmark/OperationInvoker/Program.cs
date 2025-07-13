using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

#pragma warning disable IDE1006
const int IterationCount = 25;
#pragma warning restore IDE1006

var config = DefaultConfig.Instance.AddColumn( StatisticColumn.Max )
    .AddColumn( StatisticColumn.Min )
    .AddDiagnoser( MemoryDiagnoser.Default )
    .AddDiagnoser( ThreadingDiagnoser.Default )
    .AddExporter( DefaultExporters.JsonFullCompressed )
    .AddJob( Job.Default.AsBaseline().WithId( "default" ).WithIterationCount( IterationCount ).WithPlatform( Platform.AnyCpu ) )
    .AddJob( Job.Default.WithId( "x64" ).WithIterationCount( IterationCount ).WithPlatform( Platform.X64 ) )
    .AddJob( Job.Default.WithId( "x86" ).WithIterationCount( IterationCount ).WithPlatform( Platform.X86 ).WithToolchain( InProcessEmitToolchain.Instance ) )
    .AddLogger( ConsoleLogger.Unicode );

BenchmarkSwitcher.FromAssembly( typeof( Program ).Assembly ).Run( args, config );