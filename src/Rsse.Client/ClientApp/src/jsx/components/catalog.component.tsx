import * as React from 'react';
import { LoaderComponent } from "./loader.component.tsx";
import {
    getNotesCount,
    getPageNumber,
    getCatalogPage, CatalogDto
} from "../dto/dto.catalog.tsx";

interface IState {
    data: CatalogDto|null;
}
interface IProps {}

class CatalogView extends React.Component<IProps, IState> {
    mounted: boolean;
    onDumpRenderingCounterState: number;

    public state: IState = {
    data: null
}

    constructor(props: IProps) {
        super(props);
        this.mounted = true;
        this.onDumpRenderingCounterState = 0;
    }

    componentWillUnmount() {
        this.mounted = false;
    }

    componentDidMount() {
        LoaderComponent.unusedPromise = LoaderComponent.getDataById<CatalogDto>(this, 1, LoaderComponent.catalogUrl);
    }

    click = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let target = Number(e.currentTarget.id.slice(7));
        const item = {
            "pageNumber": getPageNumber(this.state.data),
            "direction": [target]
        };
        let requestBody = JSON.stringify(item);
        LoaderComponent.unusedPromise = LoaderComponent.postData(this, requestBody, LoaderComponent.catalogUrl);
    }

    createDump = (e: React.SyntheticEvent) => {
        this.onDumpRenderingCounterState = 1;
        e.preventDefault();
        LoaderComponent.unusedPromise = LoaderComponent.getData(this, LoaderComponent.migrationCreateUrl);
    }

    restoreDump = (e: React.SyntheticEvent) => {
        this.onDumpRenderingCounterState = 1;
        e.preventDefault();
        LoaderComponent.unusedPromise = LoaderComponent.getData(this, LoaderComponent.migrationRestoreUrl);
    }

    logout = (e: React.SyntheticEvent) => {
        e.preventDefault();
        document.cookie = 'rsse_auth = false';
        let callback = (response: Response) => response.ok ? console.log("Logout Ok") : console.log("Logout Err");
        LoaderComponent.fireAndForgetWithQuery(LoaderComponent.logoutUrl, "", callback, this);
    }

    redirect = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let noteId = Number(e.currentTarget.id);
        LoaderComponent.redirectToMenu("/#/read/" + noteId);
    }

    delete = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let id = Number(e.currentTarget.id);
        console.log('You want to delete song with id: ' + id);
        LoaderComponent.unusedPromise = LoaderComponent.deleteDataById(this, id, LoaderComponent.catalogUrl, getPageNumber(this.state.data));
    }

    render() {
        if (!this.state.data) {
            return null;
        }

        let songs = [];
        let data = this.state.data;
        let itemArray = getCatalogPage(data);

        // если работаем с дампами:
        if(data.res && this.onDumpRenderingCounterState === 1)
        {
            songs.push(
                <tr key={"song "} className="bg-warning">
                    <td></td>
                    <td>{data.res}</td>
                </tr>);

            const link = document.createElement('a');
            link.href = data.res;
            link.download = data.res;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            this.onDumpRenderingCounterState = 2;
        }
        else if(data.res && this.onDumpRenderingCounterState === 2)
        {
            // если после обработки дампа нажата кнопка "Каталог":
            this.componentDidMount();
            this.onDumpRenderingCounterState = 0;
        }
        // на отладке можно получить исключение и пустой стейт:
        else if( itemArray !== null && itemArray !== undefined )
        {
            for (let i = 0; i < itemArray.length; i++) {
                songs.push(
                    <tr key={"song " + i} className="bg-warning">
                        <td></td>
                        <td>
                            <button className="btn btn-outline-light" id={ itemArray[i].item2 }
                                    onClick={ this.redirect }>{ itemArray[i].item1 }
                            </button>
                        </td>
                        <td>
                            <button className="btn btn-outline-light" id={ itemArray[i].item2 }
                                    onClick={ this.delete }>
                                &#10060;
                            </button>
                        </td>
                    </tr>);
            }
        }

        return (
            <div className="row" id="renderContainer">
                <p style={{ marginLeft: 12 + '%' }}>
                    Всего песен: { getNotesCount(this.state.data) } &nbsp;
                    Страница: { getPageNumber(this.state.data) } &nbsp;
                </p>
                <p></p>
                <p></p>
                <table className="table" id="catalogTable">
                    <thead className="thead-dark ">
                        <tr>
                            <th ></th>
                            <th >
                                <form>
                                    <button id="js-nav-1" className="btn btn-info" onClick={this.click}>
                                         &lt;Назад
                                    </button>
                                    &nbsp;
                                    <button id="js-nav-2" className="btn btn-info" onClick={this.click}>
                                          Вперёд&gt;
                                    </button>
                                    &nbsp;
                                    <button id="js-logout" className="btn btn-outline-light" onClick={this.logout}>
                                        &lt;LogOut&gt;
                                    </button>
                                    <button id="js-logout" className="btn btn-outline-light" onClick={this.createDump}>
                                        &lt;Create&gt;
                                    </button>
                                    <button id="js-logout" className="btn btn-outline-light" onClick={this.restoreDump}>
                                        &lt;Restore&gt;
                                    </button>
                                </form>
                            </th>
                            <th ></th>
                        </tr>
                    </thead>
                    <tbody>
                        {songs}
                    </tbody>
                </table>
            </div>
        );
    }
}

export default CatalogView;
