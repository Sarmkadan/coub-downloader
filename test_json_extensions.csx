#!/usr/bin/env dotnet-script

using System;
using CoubDownloader.Examples;

Console.WriteLine("Testing IntegrationExampleJsonExtensions...\n");

// Test 1: ToJson with default (non-indented)
var example = new IntegrationExample();
string jsonCompact = example.ToJson();
Console.WriteLine("Test 1 - ToJson() (compact):");
Console.WriteLine(jsonCompact);
Console.WriteLine();

// Test 2: ToJson with indented
string jsonIndented = example.ToJson(indented: true);
Console.WriteLine("Test 2 - ToJson(indented: true):");
Console.WriteLine(jsonIndented);
Console.WriteLine();

// Test 3: FromJson
try
{
    var fromJson = IntegrationExampleJsonExtensions.FromJson(jsonCompact);
    Console.WriteLine("Test 3 - FromJson():");
    Console.WriteLine("Success! Deserialized: " + (fromJson != null ? "not null" : "null"));
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine("Test 3 - FromJson() FAILED: " + ex.Message);
    Console.WriteLine();
}

// Test 4: TryFromJson with valid JSON
bool tryFromJsonSuccess = IntegrationExampleJsonExtensions.TryFromJson(jsonCompact, out var tryFromJsonValue);
Console.WriteLine("Test 4 - TryFromJson(valid JSON):");
Console.WriteLine("Success: " + tryFromJsonSuccess);
Console.WriteLine("Value: " + (tryFromJsonValue != null ? "not null" : "null"));
Console.WriteLine();

// Test 5: TryFromJson with invalid JSON
bool tryFromJsonFail = IntegrationExampleJsonExtensions.TryFromJson("invalid json", out var tryFromJsonFailValue);
Console.WriteLine("Test 5 - TryFromJson(invalid JSON):");
Console.WriteLine("Success: " + tryFromJsonFail);
Console.WriteLine("Value: " + (tryFromJsonFailValue == null ? "null (expected)" : "not null (unexpected)"));
Console.WriteLine();

// Test 6: ArgumentNullException for ToJson
try
{
    IntegrationExampleJsonExtensions.ToJson(null!);
    Console.WriteLine("Test 6 - ToJson(null) FAILED: Should have thrown ArgumentNullException");
}
catch (ArgumentNullException)
{
    Console.WriteLine("Test 6 - ToJson(null) PASSED: Correctly threw ArgumentNullException");
}
Console.WriteLine();

// Test 7: ArgumentException for FromJson with empty string
try
{
    IntegrationExampleJsonExtensions.FromJson("");
    Console.WriteLine("Test 7 - FromJson(empty) FAILED: Should have thrown ArgumentException");
}
catch (ArgumentException)
{
    Console.WriteLine("Test 7 - FromJson(empty) PASSED: Correctly threw ArgumentException");
}
Console.WriteLine();

// Test 8: ArgumentException for TryFromJson with empty string
bool tryEmpty = IntegrationExampleJsonExtensions.TryFromJson("", out var tryEmptyValue);
Console.WriteLine("Test 8 - TryFromJson(empty):");
Console.WriteLine("Success: " + !tryEmpty + " (should be false)");
Console.WriteLine();

Console.WriteLine("All tests completed!");
