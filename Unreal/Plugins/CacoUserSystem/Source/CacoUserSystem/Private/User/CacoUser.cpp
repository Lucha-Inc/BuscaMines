#include "User/CacoUserStruct.h"
#include "User/CacoUser.h"

UCacoUser::UCacoUser() {
    // Constructor vacío
}

//void UCacoUser::Init2(const FCacoUserStruct& InStruct) {}
void UCacoUser::Init2() {}


/*
void UCacoUser::Init(const FCacoUserStruct& InStruct) {
    UserId = InStruct.UserId;
    DisplayName = InStruct.DisplayName;
}

UCacoUser* UCacoUser::CreateFromStruct(UObject* Outer, const FCacoUserStruct& InStruct) {
    UCacoUser* NewUser = NewObject<UCacoUser>(Outer);
    if (NewUser) {
        NewUser->Init(InStruct);
    }
    return NewUser;
}
*/
