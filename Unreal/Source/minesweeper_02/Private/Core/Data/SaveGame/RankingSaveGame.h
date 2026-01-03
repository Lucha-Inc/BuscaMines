// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Core/Data/Struct/RankingEntry.h" // 
#include "RankingSaveGame.generated.h"

/**
 * 
 */
UCLASS()
class MINESWEEPER_02_API URankingSaveGame : public USaveGame {
	GENERATED_BODY()
	
public:
    URankingSaveGame();

    // Array de top 10
    UPROPERTY(VisibleAnywhere, BlueprintReadWrite)
    TArray<FRankingEntry> TopPlayers;
};





