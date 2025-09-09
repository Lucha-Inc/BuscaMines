#pragma once

#include "CoreMinimal.h"

class IStatsProvider;

class CACOSTATSSYSTEM_API IStatsServiceInterface
{
public:
    virtual ~IStatsServiceInterface() = default;

    // Método principal para enviar stats desde un proveedor
    virtual void SendStats(TScriptInterface<IStatsProvider> Provider) = 0;
};