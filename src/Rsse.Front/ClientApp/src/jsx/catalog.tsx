import * as React from 'react';
import { Loader } from "./loader";

interface IState {
    data: any;
}
interface IProps {
    listener: any;
}

class CatalogView extends React.Component<IProps, IState> {
    mounted: boolean;
    // стейт счетчика рендеринга компонента при обработке дампа, см. ниже:
    dumpWorkState: number;

    public state: IState = {
    data: null
}

    constructor(props: any) {
        super(props);
        this.mounted = true;
        this.dumpWorkState = 0;
    }

    componentWillUnmount() {
        this.mounted = false;
    }

    componentDidMount() {
        Loader.getDataById(this, 1, Loader.catalogUrl);
    }

    click = (e: any) => {
        e.preventDefault();
        let target = Number(e.target.id.slice(7));
        const item = {
            pageNumber: this.state.data.pageNumber,
            navigationButtons: [target]
        };
        let requestBody = JSON.stringify(item);
        Loader.postData(this, requestBody, Loader.catalogUrl);
    }

    createDump = (e: any) => {
        this.dumpWorkState = 1;
        e.preventDefault();
        Loader.getData(this, Loader.backupCreateUrl);
    }

    restoreDump = (e: any) => {
        this.dumpWorkState = 1;
        e.preventDefault();
        Loader.getData(this, Loader.backupRestoreUrl);
    }

    logout = (e: any) => {
        e.preventDefault();
        document.cookie = 'rsse_auth = false';

        let callback = (response: Response) => response.ok ? console.log("Logout Ok") : console.log("Logout Err");

        Loader.fireAndForgetWithQuery(Loader.logoutUrl, "", callback, this);
    }

    redirect = (e: any) => {
        // listener это меню.
        e.preventDefault();
        let id = Number(e.target.id);
        // Menu:
        // this.props.listener.setState({ id: id });
        Loader.redirectToMenu("/#/read/" + id);
    }

    delete = (e: any) => {
        e.preventDefault();
        let id = Number(e.target.id);
        console.log('You want to delete song with id: ' + id);
        Loader.deleteDataById(this, id, Loader.catalogUrl, this.state.data.pageNumber);
    }

    render() {
        if (!this.state.data) {
            return null;
        }

        let songs = [];

        // если работаем с дампами:
        if(this.state.data.res && this.dumpWorkState < 2)
        {
            songs.push(
                <tr key={"song "} className="bg-warning">
                    <td></td>
                    <td>{this.state.data.res}</td>
                </tr>);

            const link = document.createElement('a');
            link.href = this.state.data.res;
            link.download = this.state.data.res;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);

            this.dumpWorkState = this.dumpWorkState + 1;
        }
        else if(this.state.data.res && this.dumpWorkState >= 2)
        {
            // костыль: стейт-машина для предотвращения повторной обработки дампа при нажатии "Каталог" в процессе:
            if (this.dumpWorkState >= 2) {
                this.componentDidMount();
                this.dumpWorkState = -1;
            }

            this.dumpWorkState = this.dumpWorkState + 1;
        }
        else {
            for (let i = 0; i < this.state.data.titlesAndIds.length; i++) {
                songs.push(
                    <tr key={"song " + i} className="bg-warning">
                        <td></td>
                        <td>
                            <button className="btn btn-outline-light" id={this.state.data.titlesAndIds[i].item2}
                                    onClick={this.redirect}>{this.state.data.titlesAndIds[i].item1}
                            </button>
                        </td>
                        <td>
                            <button className="btn btn-outline-light" id={this.state.data.titlesAndIds[i].item2}
                                    onClick={this.delete}>
                                &#10060;
                            </button>
                        </td>
                    </tr>);
            }
        }

        return (
            <div className="row" id="renderContainer">
                <p style={{ marginLeft: 12 + '%' }}>
                    Всего песен: {this.state.data.songsCount} &nbsp;
                    Страница: {this.state.data.pageNumber} &nbsp;
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
                                        {/* <a href={this.state.data.res} download={this.state.data.res}>&lt;Download&gt;</a>*/}
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
