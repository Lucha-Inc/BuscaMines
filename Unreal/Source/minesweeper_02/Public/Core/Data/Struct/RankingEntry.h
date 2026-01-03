// Fill out your copyright notice in the Description page of Project Settings.


#pragma once

#include "CoreMinimal.h"
#include "RankingEntry.generated.h"

USTRUCT(BlueprintType)
struct MINESWEEPER_02_API FRankingEntry {
	GENERATED_BODY()

public:
	UPROPERTY(BlueprintReadOnly)
	FString Name;

	UPROPERTY(BlueprintReadOnly)
	float Value;

	UPROPERTY(BlueprintReadOnly)
	FDateTime Timestamp;

	FRankingEntry() : Name(TEXT("")), Value(0.f) {}
	FRankingEntry(const FString& InName, float InValue) : Name(InName), Value(InValue), Timestamp(FDateTime::Now()) {}
	FRankingEntry(const FString& InName, float InValue, const FDateTime& InTimestamp) : Name(InName), Value(InValue), Timestamp(InTimestamp) {}
};



