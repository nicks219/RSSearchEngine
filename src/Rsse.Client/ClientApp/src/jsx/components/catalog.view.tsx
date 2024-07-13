import * as React from "react";
import {CatalogResponseDto} from "../dto/request.response.dto";
import {getNotesCount, getPageNumber} from "../common/dto.handlers";

export const CatalogView = (props: {
    catalogDto:CatalogResponseDto,
    onClick:(e:React.SyntheticEvent)=>void,
    onLogout:(e:React.SyntheticEvent)=>void,
    onCreateDump:(e:React.SyntheticEvent)=>void,
    onRestoreDump:(e:React.SyntheticEvent)=>void,
    elements:JSX.Element[]}) => {
    return (
        <div className="row" id="renderContainer">
            <p style={{marginLeft: 12 + '%'}}>
                Всего песен: {getNotesCount(props.catalogDto)} &nbsp;
                Страница: {getPageNumber(props.catalogDto)} &nbsp;
            </p>
            <p></p>
            <p></p>
            <table className="table" id="catalogTable">
                <thead className="thead-dark ">
                <tr>
                    <th></th>
                    <th>
                        <form>
                            <button id="js-nav-1" className="btn btn-info" onClick={props.onClick}>
                                &lt;Назад
                            </button>
                            &nbsp;
                            <button id="js-nav-2" className="btn btn-info" onClick={props.onClick}>
                                Вперёд&gt;
                            </button>
                            &nbsp;
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onLogout}>
                                &lt;LogOut&gt;
                            </button>
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onCreateDump}>
                                &lt;Create&gt;
                            </button>
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onRestoreDump}>
                                &lt;Restore&gt;
                            </button>
                        </form>
                    </th>
                    <th></th>
                </tr>
                </thead>
                <tbody>{props.elements}</tbody>
            </table>
        </div>
    );
}
