
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "SaveGameMaster.generated.h"

UCLASS(BlueprintType)
class CACOUSERSYSTEM_API USaveGameMaster : public USaveGame {
    GENERATED_BODY()

public:

    // Guardar esta instancia en el slot del usuario
    UFUNCTION(BlueprintCallable, Category = "CacoSaveGame")
    bool Save(int32 UserId = 0) {
        FString Slot = GetSlotName(UserId, GetClass()); // usa la clase real de la instancia
        return UGameplayStatics::SaveGameToSlot(this, Slot, 0);
    }

    // Cargar el save del usuario
    UFUNCTION(BlueprintCallable, Category = "CacoSaveGame", meta = (WorldContext = "WorldContextObject"))
    static USaveGameMaster* Load(TSubclassOf<USaveGameMaster> SaveClass, int32 UserId = 0) {
        FString Slot = GetSlotName(UserId, *SaveClass);
        if (UGameplayStatics::DoesSaveGameExist(Slot, 0)) {
            return Cast<USaveGameMaster>(UGameplayStatics::LoadGameFromSlot(Slot, 0));
        }
        //return nullptr;
        return NewObject<USaveGameMaster>((UObject*)GetTransientPackage(), SaveClass);
    }

protected:

    // Genera un nombre de slot dinámico en base a la clase concreta
    static FString GetSlotName(int32 UserId, UClass* InClass) {
        return FString::Printf(TEXT("%s_%d"), *InClass->GetName(), UserId);
    }

    // Sobrecarga para TSubclassOf
    static FString GetSlotName(int32 UserId, TSubclassOf<USaveGameMaster> SaveClass) {
        return FString::Printf(TEXT("%s_%d"), *SaveClass->GetName(), UserId);
    }
};

















































/*
#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "SaveGameMaster.generated.h"

UCLASS(BlueprintType)
class CACOUSERSYSTEM_API USaveGameMaster : public USaveGame {
    GENERATED_BODY()

public:

    // Guardar esta instancia en el slot del usuario
    UFUNCTION(BlueprintCallable, Category = "SaveGame")
    bool Save(int32 UserId = 0) {
        FString Slot = GetSlotName(UserId, GetClass()); // usa la clase real de la instancia
        return UGameplayStatics::SaveGameToSlot(this, Slot, 0);
    }

    // Cargar el save del usuario
    UFUNCTION(BlueprintCallable, Category = "SaveGame")
    static USaveGameMaster* Load(TSubclassOf<USaveGameMaster> SaveClass, int32 UserId = 0) {
        FString Slot = GetSlotName(UserId, *SaveClass);
        if (UGameplayStatics::DoesSaveGameExist(Slot, 0)) {
            return Cast<USaveGameMaster>(UGameplayStatics::LoadGameFromSlot(Slot, 0));
        }
        return nullptr;
    }

protected:

    // Genera un nombre de slot dinámico en base a la clase concreta
    static FString GetSlotName(int32 UserId, UClass* InClass) {
        return FString::Printf(TEXT("%s_%d"), *InClass->GetName(), UserId);
    }

    // Sobrecarga para TSubclassOf
    static FString GetSlotName(int32 UserId, TSubclassOf<USaveGameMaster> SaveClass) {
        return FString::Printf(TEXT("%s_%d"), *SaveClass->GetName(), UserId);
    }
};
*/


























/*

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/SaveGame.h"
#include "Kismet/GameplayStatics.h"
#include "SaveGameMaster.generated.h"

UCLASS(Blueprintable)
class CACOUSERSYSTEM_API USaveGameMaster : public USaveGame {
    GENERATED_BODY()

public:

    UFUNCTION(BlueprintCallable, Category = "SaveGame")
    bool Save() {
        FString SlotName = GetClass()->GetName();
        return UGameplayStatics::SaveGameToSlot(this, SlotName, 0);
    }

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

*/



























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

















