#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "SaveGameMaster.generated.h"

UCLASS(Blueprintable)
class CACOUSERSYSTEM_API USaveGameMaster : public USaveGame {
    GENERATED_BODY()

public:

    /** Guarda esta instancia usando el nombre de la clase como slot */
    UFUNCTION(BlueprintCallable, Category = "SaveGame")
    bool Save() {
        FString SlotName = GetClass()->GetName();
        return UGameplayStatics::SaveGameToSlot(this, SlotName, 0);
    }

    /** Carga la instancia de esta clase usando el nombre de la clase como slot */
    UFUNCTION(BlueprintCallable, Category = "SaveGame")
    static USaveGameMaster* Load(TSubclassOf<USaveGameMaster> SaveClass) {
        if (!SaveClass) return nullptr;

        FString SlotName = SaveClass->GetName();

        if (UGameplayStatics::DoesSaveGameExist(SlotName, 0)) {
            USaveGame* Loaded = UGameplayStatics::LoadGameFromSlot(SlotName, 0);
            return Cast<USaveGameMaster>(Loaded);
        }

        return nullptr;
    }
};





























/*

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "SaveGameMaster.generated.h"

UCLASS(Blueprintable)
class CACOUSERSYSTEM_API USaveGameMaster : public USaveGame {
    GENERATED_BODY()

public:

    template<typename T>
    static bool Save(T* SaveInstance) {
        static_assert(TIsDerivedFrom<T, USaveGameMaster>::IsDerived, "T debe derivar de USaveGameMaster");

        if (!SaveInstance) return false;

        FString SlotName = T::StaticClass()->GetName();
        return UGameplayStatics::SaveGameToSlot(SaveInstance, SlotName, 0);
    }

    template<typename T>
    static T* Load() {
        static_assert(TIsDerivedFrom<T, USaveGameMaster>::IsDerived, "T debe derivar de USaveGameMaster");

        FString SlotName = T::StaticClass()->GetName();

        if (UGameplayStatics::DoesSaveGameExist(SlotName, 0)) {
            USaveGame* LoadedGame = UGameplayStatics::LoadGameFromSlot(SlotName, 0);
            return Cast<T>(LoadedGame);
        }

        return nullptr;
    }
};
*/

















