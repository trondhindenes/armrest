## Armrest
### Short description
Ansible Dynamic Inventory for Azure Resource Manager. This code is very non-optimized, and can be made more efficient by reducing auth calls to azure. Which I haven't gotten around to just yet.

### Description and setup:
http://hindenes.com/trondsworking/2015/08/11/using-azure-resource-manager-as-an-ansible-inventory-source/


### Tags
#### Supported tags (either on vm or resource group):

* ansible__*: the prefix will be removed, and the rest of the tag will be stamped as hostvars

* AnsibleReturn: controls whether hostname, ip address etc will be returned. Allowed values are:

        privateipaddress 

        publicipaddress

        publichostname

        privateipaddress_asansiblehost

        publicipaddress_asansiblehost

        publichostname_asansiblehost

    The _asansiblehost will retain the "real" hostname and instead use the "ansible_host" hostvar in order to allow Ansible to connect to the correct host.

* AnsibleDomainSuffix: If publichostname/publichostname_asansiblehost is used, this value contains the domain suffix to be appended to the host name

#### Other config options
See also web.config for a list of configurable values there.