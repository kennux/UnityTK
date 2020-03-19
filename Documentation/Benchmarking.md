# Benchmarking

Simplistic (micro-)benchmarking system you can use the quickly author benchmarks and execute them in the unity editor.

## How to use

In order to implement benchmarks you need to create a class inside a unity script file that implements the abstract class `UnityTK.Benchmarking.Benchmark`.
To execute your benchmarks, open the benchmarking window under Window -> UnityTK -> Benchmarking.

The benchmarks will all be listed in this editor window, by selecting them you can toggle the reports being displayed on the right side of the window.
Beyond the benchmarks list there are action buttons to execute selected or all benchmarks which will make the reports for them available.
The reload button can be used to explicitly reload the benchmarks list.

## Benchmark class

The base class for every benchmark to be implemented.
Provides 2 abstract methods to implement for running the benchmark.

The `Prepare`method can be used to prepare your data for the benchmark (for example precomputation).
The `RunBenchmark(BenchmarkResult bRes)` method can be used to actually run the benchmark.

Additionally the `PostProcessResult(ref BenchmarkResult)` can be overridden to post process the result returned by RunBenchmark.

## BenchmarkResult

This object is used to store the result of a benchmark.
It is passed into the run methods of the benchmark and can be used to create benchmarking labels (`BeginLabel(string)`).
Once begun the label must be terminated by calling `EndLabel()`. This is similar to how unity profiler sampling works.
When a label is begun, a stopwatch is started and the time till you call `EndLabel` is measured and presented in the benchmark report.

The result can also add overhead timing (usually from `PostProcessResult(ref BenchmarkResult)`) by calling the method `AddOverhead(double)`.
The overhead timing will be subtracted from the total timing to be presented as `milliseconds`.

## Unity Profiler

In order to profile your benchmarks, you can enable the unity profiler to capture in-editor and just run the benchmark as usual.
The benchmark calls will show up in the profiler with deep profiling inside some IMGUI calls on the stack.

Without deep profiling only the labels created by the benchmark will show up (they are submitted as unity profiler samples explicitly).