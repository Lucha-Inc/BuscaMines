// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/Interface.h"
#include "RankingNameInterface.generated.h"

// This class does not need to be modified.
UINTERFACE(MinimalAPI, BlueprintType)
class URankingNameInterface : public UInterface
{
	GENERATED_BODY()
};

/**
 * 
 */
class MINESWEEPER_02_API IRankingNameInterface
{
	GENERATED_BODY()

	// Add interface functions to this class. This is the class that will be inherited to implement this interface.
public:
    UFUNCTION(BlueprintCallable, BlueprintNativeEvent, Category = "Ranking")
    FString GetRankName() const;
};



