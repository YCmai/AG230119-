<template>
  <div id="app">
    <a-config-provider :locale="culture == 'en' ? en : zhCN">
      <router-view />
    </a-config-provider>
  </div>
</template>
<script>
import zhCN from "ant-design-vue/es/locale/zh_CN";
import en from "ant-design-vue/es/locale/en_US";
export default {
  name: "App",
  data() {
    return {
      zhCN,
      en,
    };
  },
  computed: {
    culture() {
      let result = this.$store.getters["global/getCulture"];
      this.$i18n.locale = result;
      return result;
    },
  },
};
</script>

<style lang="less" scope>
#app {
  height: calc(100%);
}
/deep/.ant-layout.ant-layout-has-sider {
  flex-direction: none !important;
}
</style>
