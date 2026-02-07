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







int32 UGameInstanceSubsystem_Ranking::ReportRankEntry(const FString& RankingSlot, const FString& Rank_User, float Rank_value) {
    UE_LOG(LogTemp, Warning, TEXT("ReportRankEntry called with Ranking: %s, Rank_User: %s, Rank_value: %f"), *RankingSlot, *Rank_User, Rank_value);

    // 1 Cargar ranking existente
    TArray<FRankingEntry> CurrentRanking = LoadRanking(RankingSlot);

    FDateTime EntryDatetime = FDateTime::Now();
    FRankingEntry NewEntry(Rank_User, Rank_value, EntryDatetime);
    bool bInserted = false;

    if (CurrentRanking.Num() < 10) {
        // Si hay menos de 10 elementos, agregar directamente
        CurrentRanking.Add(NewEntry);
        bInserted = true;
    } else {
        // Buscar el valor mínimo en el ranking
        int32 MinIndex = 0;
        float MinValue = CurrentRanking[0].Value;

        for (int32 i = 1; i < CurrentRanking.Num(); i++) {
            if (CurrentRanking[i].Value < MinValue) {
                MinValue = CurrentRanking[i].Value;
                MinIndex = i;
            }
        }
        //UE_LOG(LogTemp, Warning, TEXT("MIN RANK VALUE %f IN POS %i"), MinValue, MinIndex);

        // Reemplazar solo si la nueva puntuación es mayor que el mínimo
        if (Rank_value > MinValue) {
            CurrentRanking[MinIndex] = NewEntry;
            bInserted = true;
        } else {
            // No entra en el ranking ? retornamos 0
            UE_LOG(LogTemp, Warning, TEXT("Score %f no entra en el ranking del slot %s"), Rank_value, *RankingSlot);
            return 0;
        }
    }

    // 2 Ordenar ranking descendente (mayor primero)
    CurrentRanking.Sort([](const FRankingEntry& A, const FRankingEntry& B)
        {
            if (A.Value != B.Value) {
                return A.Value > B.Value; // mayor primero
            }
            return A.Timestamp < B.Timestamp; // si empate, más viejo primero
        });

    // 3 Guardar el ranking actualizado
    SaveRanking(RankingSlot, CurrentRanking);

    // 4 Determinar la posición del nuevo valor
    //int32 RankPosition = 1; // inicializamos en 1 (mayor primero)
    //for (const FRankingEntry& Entry : CurrentRanking) {
    //    if (Entry.Name == Rank_User && Entry.Value == Rank_value) {
    //        return RankPosition; // posición donde quedó la nueva puntuación
    //    }
    //    RankPosition++;
    //}

    for (int32 Index = 0; Index < CurrentRanking.Num(); Index++) {
        const FRankingEntry& Entry = CurrentRanking[Index];

        // Comprobamos que coincida nombre y valor
        if (Entry.Name == Rank_User && Entry.Value == Rank_value && Entry.Timestamp == EntryDatetime) {
            return Index + 1; // devolvemos posición 1-based
        }
    }

    // En caso improbable de no encontrarlo, retornar 0
    return 0;
}


bool UGameInstanceSubsystem_Ranking::UpdateRankName(const FString& RankingSlot, int32 Position, const FString& NewUserName) {
    // Cargar el ranking actual
    TArray<FRankingEntry> CurrentRanking = LoadRanking(RankingSlot);

    // Verificar que la posición sea válida
    if (!CurrentRanking.IsValidIndex(Position - 1)) {
        UE_LOG(LogTemp, Warning, TEXT("UpdateRankName: posicion %d fuera de rango en ranking %s"), Position, *RankingSlot);
        return false;
    }

    // Actualizar el nombre del usuario en la posición
    CurrentRanking[Position-1].Name = NewUserName;

    // Guardar cambios
    SaveRanking(RankingSlot, CurrentRanking);

    UE_LOG(LogTemp, Warning, TEXT("UpdateRankName: posicion %i en ranking %s actualizada a %s"), Position, *RankingSlot, *NewUserName);

    return true;
}









TArray<FRankingEntry> UGameInstanceSubsystem_Ranking::GetRankTop10(const FString& Ranking) const {
	UE_LOG(LogTemp, Warning, TEXT("GetRankTop10 called with Ranking: %s"), *Ranking);
	TArray<FRankingEntry> Top10Entries;
	// Ejemplo de datos ficticios

    Top10Entries = LoadRanking(Ranking);
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






FRankingEntry UGameInstanceSubsystem_Ranking::GetRankEntryAtPosition(const FString& RankingSlot, int32 Position) const {
    // Cargar ranking actual
    TArray<FRankingEntry> CurrentRanking = LoadRanking(RankingSlot);

    // Validar índice (1-based)
    if (!CurrentRanking.IsValidIndex(Position - 1)) {
        UE_LOG(LogTemp, Warning, TEXT("GetRankEntryAtPosition: posicion %d fuera de rango en ranking %s"), Position, *RankingSlot);
        return FRankingEntry(); // Devuelve un entry vacío si es inválido
    }

    // Devolver el entry
    return CurrentRanking[Position - 1];
}


