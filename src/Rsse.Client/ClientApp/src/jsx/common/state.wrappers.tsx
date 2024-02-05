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

    /** I: CatalogComponent: работа с дампами; II: CreateComponent: режима "подтверждение/отмена" */
    private static _commonState: number = 0;
    public static get commonState() {return this._commonState};
    public static set commonState(value: number) {this._commonState = value;}


    /** I: Выставляется в Login.SetVisible<T>; II: Используется для хранения JSON string с заметкой в Create в режиме "подтверждение/отмена" */
    private static _commonString: string;
    public static get commonString() {return this._commonString};
    public static set commonString(value: string) {this._commonString = value};


    /** Используется для передачи идентификатора заметки между компонентами */
    private static _commonNumber: number = 0;
    public static get commonNumber() {return this._commonNumber};
    public static set commonNumber(value: number) {this._commonNumber = value};


    /** Используется для продолжения загрузки: выставляется в Login.SetVisible<T>, восстанавливается в Login.continueLoading */
    private static _stateWrapperStorage: FunctionComponentStateWrapper<any>;
    public static get stateWrapperStorage() {return this._stateWrapperStorage};
    public static set stateWrapperStorage(value: FunctionComponentStateWrapper<any>) {this._stateWrapperStorage = value};


    /** Восстановление начальных значений */
    static init = () => {
        this._commonState = 0;
        this._commonString = "";
    }
    
}

