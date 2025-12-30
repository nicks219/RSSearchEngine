import * as React from "react";
import {CatalogResponseDto} from "../dto/catalog.response.dto.tsx";
import {getNotesCount, getPageNumber} from "../common/dto.handlers";
import {Doms} from "../dto/doms.tsx";
import {JSX} from "react";

export const CatalogView = (props: {
    catalogDto:CatalogResponseDto,
    onClick:(_e:React.SyntheticEvent)=>void,
    onLogout:(_e:React.SyntheticEvent)=>void,
    onCreateDump:(_e:React.SyntheticEvent)=>void,
    onRestoreDump:(_e:React.SyntheticEvent)=>void,
    elements:JSX.Element[]}) => {
    return (
        <div className={Doms.row} id={Doms.mainContent}>
            {/** <div id='catalog-info'>
                <p>
                    Всего песен: {getNotesCount(props.catalogDto)} &nbsp;
                    Страница: {getPageNumber(props.catalogDto)} &nbsp;
                </p>
            </div> */}

            <table className={Doms.table} id={Doms.catalogTable}>
                <thead className={Doms.theadDarkWithSpace}>
                <tr>
                    <th></th>
                    <th>
                        <form>
                            <button id="js-nav-1" className={Doms.btnBtnInfo} onClick={props.onClick}>
                                &lt;Назад
                            </button>
                            &nbsp;
                            <button id="js-nav-2" className={Doms.btnBtnInfo} onClick={props.onClick}>
                                Вперёд&gt;
                            </button>
                            &nbsp;
                            <button id="js-logout" className="btn btn-outline-light" onClick={props.onLogout}>
                                &lt;LogOut&gt;
                            </button>
                            <button id="js-logout 1" className="btn btn-outline-light" onClick={props.onCreateDump}>
                                &lt;Create&gt;
                            </button>
                            <button id="js-logout 2" className="btn btn-outline-light" onClick={props.onRestoreDump}>
                                &lt;Restore&gt;
                            </button>
                        </form>
                    </th>
                    <th></th>
                </tr>
                </thead>
                <tbody>
                {props.elements}
                <tr style={{ background: '#343a40' }}>
                    <td></td>
                    <td>
                        &nbsp;<div className={Doms.btnBtnInfo} style={{fontWeight: 'bold'}}>Страница: {getPageNumber(props.catalogDto)}</div>
                        &nbsp;<div className={Doms.btnBtnInfo} style={{fontWeight: 'bold'}}>Всего заметок: {getNotesCount(props.catalogDto)}</div>
                    </td>
                    <td></td>
                </tr>
                </tbody>
            </table>
        </div>
    );
}
