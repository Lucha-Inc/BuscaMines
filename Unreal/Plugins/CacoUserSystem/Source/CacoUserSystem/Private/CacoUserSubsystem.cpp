// CacoUserSubsystem.cpp
#include "CacoUserSubsystem.h"
#include "Kismet/GameplayStatics.h"
#include "Logging/LogMacros.h"
#include "User/CacoUserSettings.h"
#include "SaveGame/SaveGame_LastLoggedInUserIndex.h"
#include "SaveGame/SaveGame_LocalUsers.h"

DEFINE_LOG_CATEGORY_STATIC(LogCacoUserSubsystem, Log, All);





void UCacoUserSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
    Super::Initialize(Collection);

    CachedSettings = GetDefault<UCacoUserSettings>();
}

void UCacoUserSubsystem::Deinitialize()
{
    Super::Deinitialize();
}

int32 UCacoUserSubsystem::LogIn(int32 UserIndex) {
    return int32();
}











int32 UCacoUserSubsystem::GetLastLoggedInUserIndex() const
{
    USaveGame_LastLoggedInUserIndex* SaveGame_CurrentUser = Cast<USaveGame_LastLoggedInUserIndex>(USaveGameMaster::Load(USaveGame_LastLoggedInUserIndex::StaticClass()));
    return SaveGame_CurrentUser ? SaveGame_CurrentUser->LastLoggedInUserIndex : -1;
}

TMap<int32, FCacoUserStruct>& UCacoUserSubsystem::GetLocalUsers() const
{
    USaveGame_LocalUsers* LoadedSave = Cast<USaveGame_LocalUsers>( USaveGameMaster::Load(USaveGame_LocalUsers::StaticClass()) );

    if (LoadedSave) {
        for (const auto& Elem : LoadedSave->LocalUsers) {
            int32 Index = Elem.Key;
            const FCacoUserStruct& User = Elem.Value;
            UE_LOG(LogTemp, Log, TEXT("Usuario %d -> Id: %s, Nombre: %s"), Index, *User.UserId, *User.DisplayName);
        }
        return LoadedSave->LocalUsers;
    } else {
        UE_LOG(LogTemp, Warning, TEXT("No existe SaveGame_LocalUsers"));
    }
    static TMap<int32, FCacoUserStruct> EmptyArray;
    return EmptyArray;
}





















/*

bool UCacoUserSubsystem::Prueba1(int32 val) {


    // Crear instancia
    USaveGame_LastLoggedInUserIndex* SaveInstance = NewObject<USaveGame_LastLoggedInUserIndex>();

    // Asignar valor
    SaveInstance->LastLoggedInUserIndex = val;

    // Guardar usando la función Save() de la clase base
    bool bSaved = SaveInstance->Save();

    if (bSaved) {
        UE_LOG(LogTemp, Log, TEXT("SaveGame guardado correctamente"));
    } else {
        UE_LOG(LogTemp, Warning, TEXT("Error al guardar SaveGame"));
    }

    return bSaved;
}

bool UCacoUserSubsystem::Prueba2() {
    // Cargar usando la función estática Load() de la clase base
    USaveGame_LastLoggedInUserIndex* LoadedInstance =
        Cast<USaveGame_LastLoggedInUserIndex>(USaveGameMaster::Load(USaveGame_LastLoggedInUserIndex::StaticClass()));

    if (LoadedInstance) {
        int32 LastIndex = LoadedInstance->LastLoggedInUserIndex;
        UE_LOG(LogTemp, Log, TEXT("Ultimo usuario logueado: %d"), LastIndex);
        return true;
    } else {
        UE_LOG(LogTemp, Warning, TEXT("No se encontro un SaveGame previo"));
    }
    return false;
}


*/















