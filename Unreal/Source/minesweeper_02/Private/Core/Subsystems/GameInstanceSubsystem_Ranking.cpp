// Fill out your copyright notice in the Description page of Project Settings.


#include "Core/Subsystems/GameInstanceSubsystem_Ranking.h"
#include "Kismet/GameplayStatics.h"
#include "Core/Data/SaveGame/RankingSaveGame.h"


void UGameInstanceSubsystem_Ranking::Initialize(FSubsystemCollectionBase& Collection) {
	Super::Initialize(Collection);


	UE_LOG(LogTemp, Warning, TEXT("GameInstanceSubsystem inicializado"));
}

void UGameInstanceSubsystem_Ranking::Deinitialize() {
	UE_LOG(LogTemp, Warning, TEXT("GameInstanceSubsystem destruido"));


	Super::Deinitialize();
}






int32 UGameInstanceSubsystem_Ranking::ReportRankEntry(const FString& Ranking, const FString& Rank_User, float Rank_value) {
	UE_LOG(LogTemp, Warning, TEXT("ReportRankEntry called with Ranking: %s, Rank_User: %s, Rank_value: %f"), *Ranking, *Rank_User, Rank_value);

	return 0; // Retorna 0 para indicar éxito
}

TArray<FRankingEntry> UGameInstanceSubsystem_Ranking::GetRankTop10(const FString& Ranking) const {
	UE_LOG(LogTemp, Warning, TEXT("GetRankTop10 called with Ranking: %s"), *Ranking);
	TArray<FRankingEntry> Top10Entries;
	// Ejemplo de datos ficticios
	for (int32 i = 1; i <= 10; ++i) {
		FString UserName = FString::Printf(TEXT("User%d"), i);
		float Score = FMath::RandRange(0.f, 100.f);
		Top10Entries.Add(FRankingEntry(UserName, Score));
	}
	return Top10Entries;	
}


TArray<FRankingEntry> UGameInstanceSubsystem_Ranking::LoadRanking(const FString& SlotName) const {
    // Array que devolveremos
    TArray<FRankingEntry> LoadedRanking;

    // Comprobar si existe el slot
    if (UGameplayStatics::DoesSaveGameExist(SlotName, 0)) {
        URankingSaveGame* SaveGameInstance = Cast<URankingSaveGame>(
            UGameplayStatics::LoadGameFromSlot(SlotName, 0)
        );

        if (SaveGameInstance) {
            LoadedRanking = SaveGameInstance->TopPlayers;
            UE_LOG(LogTemp, Warning, TEXT("Ranking cargado desde slot: %s"), *SlotName);
        } else {
            UE_LOG(LogTemp, Warning, TEXT("Error al cargar el ranking desde slot: %s"), *SlotName);
        }
    } else {
        UE_LOG(LogTemp, Warning, TEXT("No existe ranking en el slot: %s"), *SlotName);
    }

    return LoadedRanking;

}


void UGameInstanceSubsystem_Ranking::SaveRanking(const FString& SlotName, const TArray<FRankingEntry>& RankingToSave) const {
    // Creamos el objeto SaveGame
    URankingSaveGame* SaveGameInstance = Cast<URankingSaveGame>(
        UGameplayStatics::CreateSaveGameObject(URankingSaveGame::StaticClass())
    );

    if (!SaveGameInstance) {
        UE_LOG(LogTemp, Warning, TEXT("Error al crear el SaveGame instance"));
        return;
    }

    // Asignamos el array al SaveGame
    SaveGameInstance->TopPlayers = RankingToSave;

    // Guardamos en el slot indicado
    if (UGameplayStatics::SaveGameToSlot(SaveGameInstance, SlotName, 0)) {
        UE_LOG(LogTemp, Warning, TEXT("Ranking guardado correctamente en slot: %s"), *SlotName);
    } else {
        UE_LOG(LogTemp, Warning, TEXT("Error al guardar el ranking en slot: %s"), *SlotName);
    }
}

