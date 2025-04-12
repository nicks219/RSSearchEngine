import {
    FunctionComponentStateWrapper,
    RecoveryStateStorage,
    StateTypesAlias
} from "./state.handlers";
import {Doms, SystemConstants} from "../dto/doms.tsx";

/** Изменить видимость контейнера с меню на противоположную, см. LoginBoxSetVisibility и hideMenu */
export const toggleMenuVisibility = (cssProperty: string): string => {
    if (cssProperty !== SystemConstants.none) {
        cssProperty = SystemConstants.none;
        (document.getElementById(Doms.loginName) as HTMLElement).style.display = SystemConstants.none;
        return cssProperty;
    }

    cssProperty = SystemConstants.block;
    return cssProperty;
}


/** Функционал изменения видимости контейнера с логином.
* При проявлении компонента необходимо сохранить контест восстановления. */
export const LoginBoxVisibility = (
    visibility: boolean,
    stateWrapper?: FunctionComponentStateWrapper<StateTypesAlias>,
    url?: string,
    recoveryContext?: RecoveryStateStorage<StateTypesAlias>) => {

    const SetInvisible = () => {
        const loginMessage = document.getElementById(Doms.systemMessageId) as HTMLElement;
        if (loginMessage) {
            loginMessage.style.display = SystemConstants.none;
        }
    }

    const SetVisible = () => {
        if (recoveryContext && url && stateWrapper) {
            recoveryContext.recoveryStateWrapper = stateWrapper as unknown as FunctionComponentStateWrapper<StateTypesAlias>;
            recoveryContext.recoveryString = url;
        }

        const loginMessageElement = document.getElementById(Doms.systemMessageId) as HTMLElement;
        const loginElement = document.getElementById(Doms.loginName) as HTMLElement;
        loginMessageElement.style.display = SystemConstants.block;
        loginElement.style.display = SystemConstants.block;
        // изменение css только в dom браузера, при этом стейт компонента останется submitStyleGreen:
        const submitElement = loginElement.children[0];
        submitElement.id = Doms.submitStyle;
    }

    if (visibility) {
        SetVisible();
    } else if (!visibility) {
        SetInvisible();
    }
}
