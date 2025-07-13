using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

var config = DefaultConfig.Instance.AddColumn( StatisticColumn.Max )
    .AddColumn( StatisticColumn.Min )
    .AddDiagnoser( MemoryDiagnoser.Default )
    .AddDiagnoser( ThreadingDiagnoser.Default )
    .AddExporter( DefaultExporters.JsonFullCompressed )
    .AddJob( Job.Default.AsBaseline().WithId( "default" ).WithIterationCount( 10 ).WithPlatform( Platform.AnyCpu ) )
    .AddJob( Job.Default.WithId( "x64" ).WithIterationCount( 10 ).WithPlatform( Platform.X64 ) )
    .AddJob( Job.Default.WithId( "x86" ).WithIterationCount( 10 ).WithPlatform( Platform.X86 ).WithToolchain( InProcessEmitToolchain.Instance ) )
    .AddLogger( ConsoleLogger.Unicode );

BenchmarkSwitcher.FromAssembly( typeof( Program ).Assembly ).Run( args, config );