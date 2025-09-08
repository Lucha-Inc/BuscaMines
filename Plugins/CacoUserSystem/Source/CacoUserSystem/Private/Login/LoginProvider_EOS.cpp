// LoginProviderEOS.cpp
#include "Login/LoginProvider_EOS.h"
#include "OnlineSubsystem.h"

void ULoginProvider_EOS::Initialize()
{
    if (IOnlineSubsystem* OSS = IOnlineSubsystem::Get(EOS_SUBSYSTEM))
    {
        IdentityInterface = OSS->GetIdentityInterface();
    }
}

bool ULoginProvider_EOS::Login(const FString& Username, const FString& Password)
{
    if (!IdentityInterface.IsValid()) return false;

    // Aquí iría la lógica específica de EOS
    return true;
}

void ULoginProvider_EOS::Logout()
{
    if (IdentityInterface.IsValid())
    {
        // Llamada al logout de EOS
    }
}

bool ULoginProvider_EOS::IsLoggedIn() const
{
    return IdentityInterface.IsValid() && IdentityInterface->GetLoginStatus(0) == ELoginStatus::LoggedIn;
}
