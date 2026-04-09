<template>
  <a-layout class="default-layout">
    <a-layout-header class="default-layout-header"> {{ name }} </a-layout-header>
    <a-layout-content class="default-layout-content">
      <a-menu
        mode="inline"
        :default-selected-keys="defaultSeleted"
        :defaultOpenKeys="defaultOpened"
      >
        <template v-for="menu in menus">
          <template
            v-if="!menu.meta || (menu.meta && isGranted(menu.meta.requirePermission))"
          >
            <a-sub-menu
              v-if="menu.children && menu.children.length != 0"
              :key="menu.name"
            >
              <span slot="title">
                <a-icon :type="menu.icon" /><span>{{
                  typeof menu.displayName === "function"
                    ? menu.displayName()
                    : menu.displayName
                }}</span></span
              >
              <template v-for="child in menu.children">
                <a-menu-item
                  :key="child.name"
                  v-if="
                    !child.meta || (child.meta && isGranted(child.meta.requirePermission))
                  "
                >
                  <router-link :to="child.path">
                    <a-icon :type="child.icon" />{{
                      typeof child.displayName === "function"
                        ? child.displayName()
                        : child.displayName
                    }}
                  </router-link>
                </a-menu-item>
              </template>
            </a-sub-menu>
            <a-menu-item v-else :key="menu.name" style="padding-left: 12px !important">
              <router-link :to="menu.path">
                <a-icon :type="menu.icon" />{{
                  typeof menu.displayName === "function"
                    ? menu.displayName()
                    : menu.displayName
                }}
              </router-link>
            </a-menu-item>
          </template>
        </template>
      </a-menu>
    </a-layout-content>
  </a-layout>
</template>
<script>
import component from "@/lib/base";
import locale from "@/localize/locale";
export default {
  name: "MenuSider",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  props: {
    name: {
      type: String,
      default: "",
    },
    menus: {
      type: Array,
      default: function () {
        return [];
      },
    },
  },
  computed: {
    defaultSeleted() {
      let paths = this.$route.path.match(/\w+/g);
      if (!paths || paths.length == 0) return [];
      return [paths[paths.length - 1]];
    },
    defaultOpened() {
      let paths = this.$route.path.match(/\w+/g);
      if (!paths || paths.length == 0 || paths.length < 3) return [];
      let result = [];
      this.menus.forEach((menu) => {
        if (menu.name.toLowerCase() == paths[1].toLowerCase()) result.push(menu.name);
        if (menu.items && menu.items.count > 0) {
          let subIsSelected = menu.items.filter((sub) => {
            return sub.name == paths[2];
          })[0];
          if (subIsSelected) return result.push(menu.name);
        }
      });
      return result;
    },
  },
};
</script>
<style lang="less" scoped>
/deep/.ant-menu-submenu-title {
  border-bottom: 1px solid #d9d9d9;
  padding-left: 12px !important;
}
</style>
