// This file is part of the TA.Utils project
// Copyright © 2015-2025 Timtek Systems Limited, all rights reserved.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so. The Software comes with no warranty of any kind.
// You make use of the Software entirely at your own risk and assume all liability arising from your use thereof.
// 
// File: CodeContractSpecs.cs  Last modified: 2025-06-26@10:06 by tim.long

using System;
using JetBrains.Annotations;
using Machine.Specifications;
using TA.Utils.Core;

namespace TA.Utils.Specifications;

[Subject(typeof(CodeContracts), "Throws on failed assertion")]
class when_a_code_contract_assertion_fails
{
    const                     string                         ContractFailureMessage = "Contract failed";
    const                     string                         InputValue             = "Hello";
    [CanBeNull] public static CodeContractViolationException ContractException => Exception as CodeContractViolationException;

    Because of = () => Exception = Catch.Exception(
        () => InputValue.ContractAssert(p => p.Length == 1, ContractFailureMessage)
    );

    It should_throw_contract_violation              = () => Exception.ShouldBeOfExactType<CodeContractViolationException>();
    It should_contain_the_message_from_the_contract = () => Exception.Message.ShouldEqual(ContractFailureMessage);

    It should_add_the_predicate_expression_to_the_exception =
        () => ContractException.Data["Condition"].ToString().ShouldContain("p.Length == 1");

    It should_add_the_source_value_to_the_exception = () => ContractException.Data["Value"].ShouldEqual(InputValue);
    It should_record_the_caller_member_name         = () => ContractException.Data["Caller"].ShouldEqual(nameof(of));

    static Exception Exception;
}