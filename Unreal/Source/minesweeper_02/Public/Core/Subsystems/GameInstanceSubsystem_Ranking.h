// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "Core/Data/Struct/RankingEntry.h"
#include "GameInstanceSubsystem_Ranking.generated.h"

/**
 * 
 */
UCLASS(Blueprintable)
class MINESWEEPER_02_API UGameInstanceSubsystem_Ranking : public UGameInstanceSubsystem {
	GENERATED_BODY()

public:
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	virtual void Deinitialize() override;

	UFUNCTION(BlueprintCallable, Category = "Ranking")
	int32 ReportRankEntry(const FString& Ranking, const FString& Rank_User, float Rank_value);

	UFUNCTION(BlueprintCallable, Category = "Ranking")
	bool UpdateRankName(const FString& RankingSlot, int32 Position, const FString& NewUserName);

	UFUNCTION(BlueprintCallable, Category = "Ranking")
	TArray<FRankingEntry> GetRankTop10(const FString& Ranking) const;

private:
	// Carga el ranking desde un slot específico
	TArray<FRankingEntry> LoadRanking(const FString& SlotName) const;

	// Guarda un ranking en un slot específico
	void SaveRanking(const FString& SlotName, const TArray<FRankingEntry>& RankingToSave) const;


};


