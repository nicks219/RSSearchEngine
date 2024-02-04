import * as React from "react";
import {Dispatch, SetStateAction} from "react";

/** Обёртка, позволяющая загрузчику менять стейт функциональных компонентов приложения */
export class FunctionComponentStateWrapper<T> {
    public mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>];
    public setData: Dispatch<SetStateAction<T|null>>;

    constructor(mounted: [boolean, React.Dispatch<React.SetStateAction<boolean>>],
                setData: Dispatch<SetStateAction<T|null>>) {
        this.mounted = mounted;
        this.setData = setData;
    }
}

// TODO: сократи количество полей для внешнего стейта

/** Глобальный стейт с некоторой инициализацией */
export class StateStorageWrapper {
    private static _state: number = 0;

    /** ReadComponent: вызов (редирект) из каталога */
    public static renderedAfterRedirect: boolean = false;
    public static redirectCall: boolean = false;

    /** CreateComponent.SubmitButton state: */
    public static submitStateStorage? : number;
    public static requestBodyStorage: string = "";

    static setState = (state: number) => {
        StateStorageWrapper._state = state;
    }
    static getState = () => StateStorageWrapper._state;
}

/** Дополнительный глобальный стейт */
declare global {
    interface Window {
        /** передача номера заметки между компонентами */
        noteIdStorage: number,
        /** выставляется в SetVisible<T> */
        stateWrapperStorage: FunctionComponentStateWrapper<any>,
        /** выставляется в SetVisible<T> */
        urlStorage: string
    }
}
