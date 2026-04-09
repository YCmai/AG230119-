export default {
    //根据参数名获取对应的url参数
    queryGet: function(url, name) {
        let reg = new RegExp("([\?|\&)])" + name + "=([^&]*)(&|$)", "i");
        let r = url.substr(1).match(reg);
        if (r != null) return unescape(r[2]);
        return null;
    },
    normalize: function(query) {
        let strQuery = "";
        for (let p in query) {
            strQuery += `&${p}=${query[p]}`;
        }
        strQuery = strQuery.slice(1);
        return strQuery;
    }
}