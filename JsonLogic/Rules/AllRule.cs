﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `all` operation.
/// </summary>
[Operator("all")]
[JsonConverter(typeof(AllRuleJsonConverter))]
public class AllRule : Rule
{
	/// <summary>
	/// The sequence of elements to apply the rule to.
	/// </summary>
	protected internal Rule Input { get; }
	
	/// <summary>
	/// The rule to apply to All items in the input sequence.
	/// </summary>
	protected internal Rule Rule { get; }

	/// <summary>
	/// Creates a new instance of <see cref="AllRule"/> when 'all' operator is detected within json logic.
	/// </summary>
	/// <param name="input">A sequence of elements to apply the rule to.</param>
	/// <param name="rule">A rule to apply to All items in the input sequence.</param>
	protected internal AllRule(Rule input, Rule rule)
	{
		Input = input;
		Rule = rule;
	}

	/// <summary>
	/// Applies the rule to the input data.
	/// </summary>
	/// <param name="data">The input data.</param>
	/// <param name="contextData">
	///     Optional secondary data.  Used by a few operators to pass a secondary
	///     data context to inner operators.
	/// </param>
	/// <returns>The result of the rule.</returns>
	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = Input.Apply(data, contextData);

		if (input is not JsonArray arr) return false;

		var results = arr.Select(value => Rule.Apply(contextData, value)).ToList();
		return (results.Count != 0 &&
				results.All(result => result.IsTruthy()));
	}
}

internal class AllRuleJsonConverter : WeaklyTypedJsonConverter<AllRule>
{
	public override AllRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length:2})
			throw new JsonException("The all rule needs an array with 2 parameters.");

		return new AllRule(parameters[0], parameters[1]);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "We guarantee that the SerializerOptions covers all the types we need for AOT scenarios.")]
	public override void Write(Utf8JsonWriter writer, AllRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("all");
		writer.WriteStartArray();
		options.Write(writer, value.Input, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Rule, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
