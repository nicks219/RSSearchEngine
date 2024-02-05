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

/** Глобальный стейт с начальной инициализацией некоторых полей */
export class CommonStateStorage {
    /** Передача номера заметки между компонентами */
    public static noteIdStorage: number = 0;
    /** Выставляется в SetVisible<T> */
    public static stateWrapperStorage: FunctionComponentStateWrapper<any>;


    /** I: CatalogComponent: работа с дампами; II: CreateComponent: режима "подтверждение/отмена" */
    private static _commonState: number = 0;
    public static get commonState() {return this._commonState};
    public static set commonState(value: number) {this._commonState = value;}


    /** I: Выставляется в Login.SetVisible<T>; II: Используется для хранения JSON string с заметкой в Create в режиме "подтверждение/отмена" */
    public static _commonString: string;
    public static get commonString() {return this._commonString};
    public static set commonString(value: string) {this._commonString = value};


    /** Восстановление начальных значений */
    static init = () => {
        this._commonState = 0;
        this._commonString = "";
    }
}

