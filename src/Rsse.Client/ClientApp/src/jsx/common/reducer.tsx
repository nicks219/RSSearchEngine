import {connect} from "react-redux";
import React from "react";

// REACT-REDUX: демонстрационный код для react-redux:

// CONSTANTS:
export const GET_COMMON_STATE: string = "get_common_state";
export const SET_COMMON_STATE: string = "set_common_state";

// ACTIONS:
export const getCommonState = (): {type:string} => ({
    type: GET_COMMON_STATE
});
export const setCommonState = (value:number): {type:string,value:number} => ({
    type: SET_COMMON_STATE,
    value: value
});

// REDUCER:
export const reducer = (state: {commonState: number}, action: any): /*{commonState: number}*/any => {
    if (!state) {
        return {commonState: 0};
    }

    switch (action.type){
        case GET_COMMON_STATE:
            return {
                commonState: state.commonState - 1
            };
        case SET_COMMON_STATE:
            return {
                commonState: action.value
            };
        default:
            return state;
    }
}

// две функции формируют набор props для вызова компонента:
// I. обновления хранилища передаётся компоненту в виде свойств
// подписка и ре-рендер при новых данных: вопрос - как при необходимости не рендерить заново компонент?
const mapStateToProps = (state: any) => ({
    payload: state.commonState
});
// II. доступ к действиям передаётся компоненту в виде свойств:
const mapDispatchToProps = (dispatch: any) => ({
    onSet(value: React.SyntheticEvent): void {
        const attributes = value.currentTarget.attributes;
        const number = Number(attributes[0].value);
        dispatch(setCommonState(number))
    },
    onGet(): void {
        dispatch(getCommonState())
    }
});

// PRESENTATIONAL COMPONENT: компонент, находящийся в контексте провайдера:
export const PresentationalComponent = (props: any) => {
    return (
        <div>
            <button onClick={props.onSet} value={10}>=</button>
            <button onClick={props.onGet}>-</button>
            <span>{props.payload}</span>
        </div>
    );
}

// CONTAINER COMPONENT: подключаем презентационный компонент к хранилищу redux:
export const ContainerComponent = connect(
    mapStateToProps,
    mapDispatchToProps
)(PresentationalComponent);
